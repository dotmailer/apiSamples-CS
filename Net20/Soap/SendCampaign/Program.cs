using System;
using System.Net;
using System.Threading;
using dotMailer.SendCampaign.dotMailer;

namespace dotMailer.SendCampaign
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static Secure_ApiService _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetSoapClient();

            ApiAddressBook addressBook = CreateAddressBook();
            AddContactToAddressBook(addressBook);

            ApiCampaign campaign = CreateCampaign();

            ApiCampaignSend sendResult = SendCampaignToAddressBook(campaign, addressBook);
            WaitUntilSendFinishes(sendResult);

            PrintCampaingSummary(campaign);
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private static ApiAddressBook CreateAddressBook()
        {
            ApiAddressBook addressBook = new ApiAddressBook
            {
                Name = Guid.NewGuid().ToString()
            };

            ApiAddressBook createdAddressBook = _client.CreateAddressBook(addressBook);
            Console.WriteLine("Address book '{0}' has been created", createdAddressBook.Name);
            return createdAddressBook;
        }

        private static void AddContactToAddressBook(ApiAddressBook addressBook)
        {
            ApiContact contact = new ApiContact
            {
                Email = string.Format("email{0}@example.com", Guid.NewGuid()),
            };

            ApiContact addedContact = _client.AddContactToAddressBook(addressBook.Id, true, contact);
            Console.WriteLine("Contact '{0}' has been added to address book '{1}'", addedContact.Email, addressBook.Name);
        }

        private static ApiCampaign CreateCampaign()
        {
            ApiCampaign campaign = new ApiCampaign
            {
                Name = "My campaign",
                Subject = "Subject",
                FromName = "Friendly name",
                HtmlContent = "<a href=\"http://$UNSUB$\">Unsubscribe</a>",
                PlainTextContent = "Unsubscribe $UNSUB$"
            };

            ApiCampaign createdCampaign = _client.CreateCampaign(campaign);
            Console.WriteLine("Campaign '{0}' has been created", createdCampaign.Name);
            return createdCampaign;
        }

        private static ApiCampaignSend SendCampaignToAddressBook(ApiCampaign campaign, ApiAddressBook addressBook)
        {
            ApiCampaignSend campaignSend = new ApiCampaignSend
            {
                CampaignId = campaign.Id,
                AddressBookIds = new[] { addressBook.Id }
            };

            ApiCampaignSend sendResult = _client.SendCampaign(campaignSend);
            Console.WriteLine("Campaign '{0}' has been sended to address book '{1}'", campaign.Name, addressBook.Name);
            return sendResult;
        }

        protected static void WaitUntilSendFinishes(ApiCampaignSend campaignSend)
        {
            ApiCampaignSendStatuses status = campaignSend.Status;
            while (status == ApiCampaignSendStatuses.NotSent ||
                   status == ApiCampaignSendStatuses.Sending ||
                   status == ApiCampaignSendStatuses.Scheduled)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                ApiCampaignSend progress = _client.GetCampaignSendProgress(campaignSend.Id);
                status = progress.Status;
            }
        }

        private static Secure_ApiService GetSoapClient()
        {
            Secure_ApiService client = new Secure_ApiService();
            client.Credentials = new NetworkCredential(UserName, Password);
            return client;
        }

        private static void PrintCampaingSummary(ApiCampaign campaign)
        {
            ApiCampaignSummary summary = _client.GetCampaignSummary(campaign.Id, true);
            Console.WriteLine("Campaign has been sended {0} times", summary.NumSent);
        }
    }
}
