using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace dotMailer.EditContact
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            ApiContact contact = CreateContact();
            PrintContact(contact);

            contact = UpdateContact(contact);
            PrintContact(contact);
            
            DeleteContact(contact.Id);
        }

        private static void DeleteContact(int contactId)
        {
            string url = "https://apiconnector.com/v2/contacts/" + contactId;
            WebRequest request = CreateRequest(url);
            request.Method = "DELETE";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new ApplicationException("Can't delete contact");
                }
            }
        }

        private static ApiContact UpdateContact(ApiContact contact)
        {
            contact = UpdateEmail(contact);

            string url = "https://apiconnector.com/v2/contacts/" + contact.Id;
            WebRequest request = CreateRequestWithPayload(contact, url, "PUT");
            contact = ReadResult<ApiContact>(request);

            return contact;
        }

        private static ApiContact CreateContact()
        {
            ApiContact contact = CreateContactEntity();
            WebRequest request = CreateRequestWithPayload(contact, "https://apiconnector.com/v2/contacts", "POST");
            contact = ReadResult<ApiContact>(request);    
        
            return contact;
        }

        private static WebRequest CreateRequestWithPayload(ApiContact contact, string url, string method)
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

        private static ApiContact UpdateEmail(ApiContact contact)
        {
            contact.Email = String.Format("email-{0}@example.com", Guid.NewGuid().ToString("N"));         

            return contact;
        }
        
        private static WebRequest CreateRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);
            String base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password));
            request.Headers.Add("Authorization", "Basic " + base64);

            return request;
        }

        private static void PrintContact(ApiContact contact)
        {
            Console.WriteLine("Email = {0}", contact.Email);
            Console.WriteLine("Id = {0}", contact.Id);
            Console.WriteLine("OptInType = {0}", contact.OptInType);
            Console.WriteLine("EmailType = {0}", contact.EmailType);

            foreach (ApiContactData data in contact.DataFields)
            {
                Console.WriteLine("{0,-20} = {1}", data.Key, data.Value);
            }

            Console.WriteLine();
        }

        private static ApiContact CreateContactEntity()
        {
            ApiContact contact = new ApiContact
            {
                Email = String.Format("email-{0}@example.com", Guid.NewGuid().ToString("N")),
                EmailType = ApiContactEmailTypes.Html,
                OptInType = ApiContactOptInTypes.Double,
                DataFields = new ApiContactData[]
                        {
                            new ApiContactData{Key = "Gender", Value = "M"},
                            new ApiContactData{Key = "FullName", Value = "John Smith"}
                        }
            };

            return contact;
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
