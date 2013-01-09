using System;
using System.Net;
using System.Web.Services.Protocols;
using dotMailer.EditContact.dotMailer;

namespace dotMailer.EditContact
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

            ApiContact contact = CreateContact();
            PrintContact(contact);

            contact = UpdateEmail(contact);
            PrintContact(contact);

            DeleteContact(contact.Id);
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
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
            ApiContact result = _client.CreateContact(contact);

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

        private static ApiContact UpdateEmail(ApiContact contact)
        {
            contact.Email = String.Format("email-{0}@example.com", Guid.NewGuid().ToString("N"));
            ApiContact result = _client.UpdateContact(contact);

            return result;
        }

        private static Secure_ApiService GetSoapClient()
        {
            Secure_ApiService client = new Secure_ApiService();
            client.Credentials = new NetworkCredential(UserName, Password);
            return client;
        }

        private static void DeleteContact(int contactId)
        {
            try
            {
                _client.DeleteContact(contactId, true);
            }
            catch (SoapException)
            {
                throw new ApplicationException("Can't delete contact");
            }
        }
    }
}
