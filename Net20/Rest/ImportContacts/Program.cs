using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace dotMailer.ImportContacts
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";
        private const int AddressBookId = 0; //Your address book here

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            ApiContactImport contactImport = ImportContactsFromFile();
            ApiContactImport contactImportResult = WaitUntilImportFinishes(contactImport);
            PrintContactImport(contactImportResult);

            Dictionary<String, String> contactImportReport = GetContactImportReport(contactImportResult.Id);
            PrintContactImportReport(contactImportReport);
            
            String csvReport = GetContactImportFaults(contactImportResult.Id);
            PrintCsvReport(csvReport);
        }

        private static String GetContactImportFaults(Guid importId)
        {
            string url = String.Format("https://apiconnector.com/v2/contacts/import/{0}/report-faults", importId);
            HttpWebRequest request = CreateRequest(url);
            String result = ReadResultAsString(request);
            return result;
        }

        private static void PrintCsvReport(string csvReport)
        {
            Console.WriteLine();
            Console.WriteLine(csvReport);
        }

        private static Dictionary<string, string> GetContactImportReport(Guid importId)
        {
            string url = String.Format("https://apiconnector.com/v2/contacts/import/{0}/report", importId);
            HttpWebRequest request = CreateRequest(url);
            Dictionary<string, string>  result = ReadResult<Dictionary<string, string>>(request);
            return result;
        }

        private static ApiContactImport WaitUntilImportFinishes(ApiContactImport importResult)
        {
            ApiContactImport result = importResult;

            while (result.Status == ApiContactImportStatuses.NotFinished)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                HttpWebRequest request = CreateRequest("https://apiconnector.com/v2/contacts/import/" + importResult.Id);
                result = ReadResult<ApiContactImport>(request);
            }

            return result;
        }

        private static ApiContactImport ImportContactsFromFile()
        {
            string url = String.Format("https://apiconnector.com/v2/address-books/{0}/contacts/import", AddressBookId);
            HttpWebRequest request = CreateRequest(url);
            request.Method = "POST";

            AddContactsToRequest(request);

            ApiContactImport result = ReadResult<ApiContactImport>(request);
            return result;
        }

        private static void AddContactsToRequest(HttpWebRequest request)
        {
            String boundary = Guid.NewGuid().ToString("N");
            request.ContentType = String.Format(@"multipart/form-data; boundary=""{0}""", boundary);

            using (BinaryWriter writer = new BinaryWriter(request.GetRequestStream()))
            {                
                writer.Write(Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n"));

                const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n\r\n";
                string header = string.Format(headerTemplate, "Contacts", "Contacts.csv");

                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                writer.Write(headerbytes);

                byte[] contacts = File.ReadAllBytes("Contacts.csv");
                writer.Write(contacts);

                writer.Write(Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n"));
            }
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            String base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password));
            request.Headers.Add("Authorization", "Basic " + base64);

            return request;
        }

        private static void PrintContactImport(ApiContactImport importResult)
        {
            Console.WriteLine("Import Id = {0}", importResult.Id);
            Console.WriteLine("Import Status = {0}", importResult.Status);
        }

        private static void PrintContactImportReport(Dictionary<String, String> contactImportReport)
        {
            Console.WriteLine();
            foreach (KeyValuePair<string, string> pair in contactImportReport)
            {
                Console.WriteLine("{0,-20} = {1}", pair.Key, pair.Value);
            }
        }

        private static String ReadResultAsString(WebRequest request)
        {
            String result;

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
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
