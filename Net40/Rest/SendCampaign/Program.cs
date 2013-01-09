using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace dotMailer.SendCampaign
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static HttpClient _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;
            _client = GetHttpClient();

            ApiAddressBook addressBook = CreateAddressBook();
            AddContactToAddressBook(addressBook);

            ApiCampaign campaign = CreateCampaign();

            ApiCampaignSend sendResult = SendCampaignToAddressBook(campaign, addressBook);
            WaitUntilSendFinishes(sendResult);

            PrintCampaingSummary(campaign);
        }

        protected static void WaitUntilSendFinishes(ApiCampaignSend campaignSend)
        {
            ApiCampaignSendStatuses status = campaignSend.Status;
            while (status == ApiCampaignSendStatuses.NotSent ||
                   status == ApiCampaignSendStatuses.Sending ||
                   status == ApiCampaignSendStatuses.Scheduled)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                HttpResponseMessage response = _client.GetAsync("/v2/campaigns/send/" + campaignSend.Id).Result;
                ApiCampaignSend progress = response.Content.ReadAsAsync<ApiCampaignSend>().Result;
                status = progress.Status;
            }
        }

        private static ApiCampaignSend SendCampaignToAddressBook(ApiCampaign campaign, ApiAddressBook addressBook)
        {
            ApiCampaignSend campaignSend = new ApiCampaignSend
            {
                CampaignId = campaign.Id,
                AddressBookIds = new[] { addressBook.Id }
            };

            HttpResponseMessage response = _client.PostAsJsonAsync("/v2/campaigns/send", campaignSend).Result;
            ApiCampaignSend sendResult = response.Content.ReadAsAsync<ApiCampaignSend>().Result;
            Console.WriteLine("Campaign '{0}' has been sended to address book '{1}'", campaign.Name, addressBook.Name);
            return sendResult;
        }

        private static void PrintCampaingSummary(ApiCampaign campaign)
        {
            String url = String.Format("v2/campaigns/{0}/summary", campaign.Id); 
            HttpResponseMessage response = _client.GetAsync(url).Result;
            Dictionary<String, String> summary = response.Content.ReadAsAsync<Dictionary<String, String>>().Result;
            Console.WriteLine("Campaign has been sended {0} times", summary["NumSent"]);
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

            HttpResponseMessage response = _client.PostAsJsonAsync("/v2/campaigns", campaign).Result;
            ApiCampaign createdCampaign = response.Content.ReadAsAsync<ApiCampaign>().Result;
            Console.WriteLine("Campaign '{0}' has been created", createdCampaign.Name);
            return createdCampaign;
        }

        private static ApiAddressBook CreateAddressBook()
        {
            ApiAddressBook addressBook = new ApiAddressBook
            {
                Name = Guid.NewGuid().ToString()
            };

            HttpResponseMessage response = _client.PostAsJsonAsync("/v2/address-books", addressBook).Result;
            ApiAddressBook createdAddressBook = response.Content.ReadAsAsync<ApiAddressBook>().Result;
            Console.WriteLine("Address book '{0}' has been created", createdAddressBook.Name);
            return createdAddressBook;
        }

        private static void AddContactToAddressBook(ApiAddressBook addressBook)
        {
            ApiContact contact = new ApiContact
            {
                Email = string.Format("email{0}@example.com", Guid.NewGuid())
            };

            string url = String.Format("v2/address-books/{0}/contacts", addressBook.Id);
            HttpResponseMessage response = _client.PostAsJsonAsync(url, contact).Result;
            ApiContact addedContact = response.Content.ReadAsAsync<ApiContact>().Result;
            Console.WriteLine("Contact '{0}' has been added to address book '{1}'", addedContact.Email, addressBook.Name);
        }

        private static HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://apiconnector.com/");

            String base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

            return client;
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
