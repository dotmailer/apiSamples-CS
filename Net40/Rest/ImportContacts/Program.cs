using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace dotMailer.ImportContacts
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";
        private const int AddressBookId = 0; //Your address book here

        private static HttpClient _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetHttpClient();
            
            MultipartFormDataContent contacts = GetContactsForImport();
            ApiContactImport contactImport = ImportContacts(contacts);
            ApiContactImport contactImportResult = WaitUntilImportFinishes(contactImport);
            PrintContactImport(contactImportResult);

            Dictionary<String, String> contactImportReport = GetContactImportReport(contactImportResult.Id);
            PrintContactImportReport(contactImportReport);

            String csvReport = GetContactImportFaults(contactImportResult.Id);
            PrintCsvReport(csvReport);
        }

        private static void PrintCsvReport(string csvReport)
        {
            Console.WriteLine();
            Console.WriteLine(csvReport);
        }

        private static String GetContactImportFaults(Guid importId)
        {
            String url = String.Format("/v2/contacts/import/{0}/report-faults", importId);
            HttpResponseMessage response = _client.GetAsync(url).Result;
            String csvReport = response.Content.ReadAsStringAsync().Result;
            return csvReport;
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private static void PrintContactImportReport(Dictionary<String, String> contactImportReport)
        {
            Console.WriteLine();
            foreach (KeyValuePair<string, string> pair in contactImportReport)
            {
                Console.WriteLine("{0,-20} = {1}", pair.Key, pair.Value);
            }
        }

        private static Dictionary<string, string> GetContactImportReport(Guid importId)
        {
            String url = String.Format("/v2/contacts/import/{0}/report", importId);
            HttpResponseMessage response = _client.GetAsync(url).Result;
            Dictionary<string, string> result = response.Content.ReadAsAsync<Dictionary<string, string>>().Result;
            return result;
        }

        private static ApiContactImport ImportContacts(MultipartFormDataContent contacts)
        {
            string url = String.Format("/v2/address-books/{0}/contacts/import", AddressBookId);
            HttpResponseMessage response = _client.PostAsync(url, contacts).Result;
            ApiContactImport contactImport = response.Content.ReadAsAsync<ApiContactImport>().Result;
            return contactImport;
        }

        private static void PrintContactImport(ApiContactImport importResult)
        {
            Console.WriteLine("Import Id = {0}", importResult.Id);
            Console.WriteLine("Import Status = {0}", importResult.Status);
        }

        private static ApiContactImport WaitUntilImportFinishes(ApiContactImport importResult)
        {
            ApiContactImport result = importResult;

            while (result.Status == ApiContactImportStatuses.NotFinished)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                string url = "/v2/contacts/import/" + importResult.Id;
                HttpResponseMessage response = _client.GetAsync(url).Result;
                result = response.Content.ReadAsAsync<ApiContactImport>().Result;
            }

            return result;
        }

        private static MultipartFormDataContent GetContactsForImport()
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            byte[] contacts = File.ReadAllBytes("Contacts.csv");
            content.Add(new ByteArrayContent(contacts), "Contacts", "Contacts.csv");
            return content;
        }

        private static HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://apiconnector.com/");

            String base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

            return client;
        }
    }
}
