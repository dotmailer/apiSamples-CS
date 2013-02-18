using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace dotMailer.SendCampaignWithAttachment
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            ApiDocumentFolder folder = CreateDocumentFolder();
            ApiDocument document = UploadDocument(folder.Id);

            ApiCampaign campaign = CreateCampaign();
            AttachDocument(campaign, document);

            ApiAddressBook addressBook = CreateAddressBook();
            AddContactToAddressBook(addressBook);

            ApiCampaignSend sendResult = SendCampaignToAddressBook(campaign, addressBook);
            WaitUntilSendFinishes(sendResult);

            PrintCampaingSummary(campaign);
        }

        private static ApiDocument UploadDocument(int documentFolderId)
        {
            string url = String.Format("https://apiconnector.com/v2/document-folders/{0}/documents", documentFolderId);
            HttpWebRequest request = CreateRequest(url);
            request.Method = "POST";

            AddDocumentToRequest(request);

            ApiDocument result = ReadResult<ApiDocument>(request);
            return result;
        }

        private static void AddDocumentToRequest(HttpWebRequest request)
        {
            const String fileName = "SampleDocument.txt";

            String boundary = Guid.NewGuid().ToString("N");
            request.ContentType = String.Format(@"multipart/form-data; boundary=""{0}""", boundary);

            using (BinaryWriter writer = new BinaryWriter(request.GetRequestStream()))
            {
                writer.Write(Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n"));

                const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n\r\n";
                string header = string.Format(headerTemplate, "SampleDocument", fileName);

                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                writer.Write(headerbytes);

                byte[] contacts = File.ReadAllBytes(fileName);
                writer.Write(contacts);

                writer.Write(Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n"));
            }
        }

        private static void AttachDocument(ApiCampaign campaign, ApiDocument document)
        {
            String url = String.Format("https://apiconnector.com/v2/campaigns/{0}/attachments", campaign.Id);
            WebRequest request = CreateRequestWithPayload(document, url, "POST");
            
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new ApplicationException("Can't attach document to campaign");
                }
            }
        }

        private static ApiDocumentFolder CreateDocumentFolder()
        {
            ApiDocumentFolder documentFolder = new ApiDocumentFolder
            {
                Name = "Attachments",
            };

            WebRequest request = CreateRequestWithPayload(documentFolder, "https://apiconnector.com/v2/document-folders/0", "POST");
            ApiDocumentFolder result = ReadResult<ApiDocumentFolder>(request);

            return result;
        }

        protected static void WaitUntilSendFinishes(ApiCampaignSend campaignSend)
        {
            ApiCampaignSendStatuses status = campaignSend.Status;
            while (status == ApiCampaignSendStatuses.NotSent ||
                   status == ApiCampaignSendStatuses.Sending ||
                   status == ApiCampaignSendStatuses.Scheduled)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                WebRequest request = CreateRequest("https://apiconnector.com/v2/campaigns/send/" + campaignSend.Id);
                ApiCampaignSend progress = ReadResult<ApiCampaignSend>(request);

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

            WebRequest request = CreateRequestWithPayload(campaignSend, "https://apiconnector.com/v2/campaigns/send", "POST");
            ApiCampaignSend sendResult = ReadResult<ApiCampaignSend>(request);
            Console.WriteLine("Campaign '{0}' has been sended to address book '{1}'", campaign.Name, addressBook.Name);

            return sendResult;
        }

        private static WebRequest CreateRequestWithPayload<TEntity>(TEntity contact, string url, string method)
        {
            WebRequest request = CreateRequest(url);
            request.Method = method;
            request.ContentType = "application/json";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                Serializer.Serialize(writer, contact);
            }

            return request;
        }

        private static void PrintCampaingSummary(ApiCampaign campaign)
        {
            String url = String.Format("https://apiconnector.com/v2/campaigns/{0}/summary", campaign.Id);
            WebRequest request = CreateRequest(url);

            Dictionary<String, String> summary = ReadResult<Dictionary<String, String>>(request);

            Console.WriteLine("Campaign has been sended {0} times", summary["numSent"]);
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

            WebRequest request = CreateRequestWithPayload(campaign, "https://apiconnector.com/v2/campaigns", "POST");
            ApiCampaign createdCampaign = ReadResult<ApiCampaign>(request);

            Console.WriteLine("Campaign '{0}' has been created", createdCampaign.Name);
            return createdCampaign;
        }

        private static ApiAddressBook CreateAddressBook()
        {
            ApiAddressBook addressBook = new ApiAddressBook
            {
                Name = Guid.NewGuid().ToString()
            };

            WebRequest request = CreateRequestWithPayload(addressBook, "https://apiconnector.com/v2/address-books", "POST");
            ApiAddressBook createdAddressBook = ReadResult<ApiAddressBook>(request);

            Console.WriteLine("Address book '{0}' has been created", createdAddressBook.Name);
            return createdAddressBook;
        }

        private static void AddContactToAddressBook(ApiAddressBook addressBook)
        {
            ApiContact contact = new ApiContact
            {
                Email = string.Format("email{0}@example.com", Guid.NewGuid())
            };

            string url = String.Format("https://apiconnector.com/v2/address-books/{0}/contacts", addressBook.Id);
            WebRequest request = CreateRequestWithPayload(contact, url, "POST");
            ApiContact addedContact = ReadResult<ApiContact>(request);

            Console.WriteLine("Contact '{0}' has been added to address book '{1}'", addedContact.Email, addressBook.Name);
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            String base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password));
            request.Headers.Add("Authorization", "Basic " + base64);

            return request;
        }

        private static TResult ReadResult<TResult>(WebRequest request)
        {
            TResult result;

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        result = serializer.Deserialize<TResult>(jsonReader);
                    }
                }
            }

            return result;
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
