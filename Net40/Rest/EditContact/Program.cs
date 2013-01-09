using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace dotMailer.EditContact
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

            ApiContact contact = CreateContact();
            PrintContact(contact);

            contact = UpdateEmail(contact);
            PrintContact(contact);

            DeleteContact(contact.Id);
        }

        private static void DeleteContact(int contactId)
        {
            String url = String.Format("/v2/contacts/{0}", contactId);
            HttpResponseMessage response = _client.DeleteAsync(url).Result;
            
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException("Can't delete contact");
            }
        }

        private static ApiContact UpdateEmail(ApiContact contact)
        {
            contact.Email = String.Format("email-{0}@example.com", Guid.NewGuid().ToString("N"));

            String url = String.Format("/v2/contacts/{0}", contact.Id);
            HttpResponseMessage response = _client.PutAsJsonAsync(url, contact).Result;
            ApiContact result = response.Content.ReadAsAsync<ApiContact>().Result;

            return result;
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

        private static ApiContact CreateContact()
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

            HttpResponseMessage response = _client.PostAsJsonAsync("/v2/contacts", contact).Result;
            ApiContact result = response.Content.ReadAsAsync<ApiContact>().Result;
            
            return result;
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
