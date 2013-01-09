using System;
using System.IO;
using System.Threading;
using dotMailer.ImportContacts.ApiServiceReference;

namespace dotMailer.ImportContacts
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";
        private const int AddressBookId = 0; //Your address book here

        private static ApiServiceClient _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetWcfClient();

            byte[] contacts = GetContactsForImport();
            ApiContactImport contactImport = _client.ImportContactsToAddressBook(AddressBookId, contacts, "csv");
            ApiContactImport contactImportResult = WaitUntilImportFinishes(contactImport);
            PrintContactImport(contactImportResult);

            ApiContactImportReport contactImportReport = _client.GetContactImportReport(contactImportResult.Id);
            PrintContactImportReport(contactImportReport);

            string csvReport = GetContactImportFaults(contactImportResult.Id);
            PrintCsvReport(csvReport);
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private static ApiServiceClient GetWcfClient()
        {
            ApiServiceClient client = new ApiServiceClient("Secure_ApiService");
            client.ClientCredentials.UserName.UserName = UserName;
            client.ClientCredentials.UserName.Password = Password;
            return client;
        }

        private static byte[] GetContactsForImport()
        {
            return File.ReadAllBytes("Contacts.csv");
        }

        private static ApiContactImport WaitUntilImportFinishes(ApiContactImport importResult)
        {
            ApiContactImport result = importResult;

            while (result.Status == ApiContactImportStatuses.NotFinished)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                result = _client.GetContactImportProgress(importResult.Id);
            }

            return result;
        }

        private static void PrintContactImport(ApiContactImport importResult)
        {
            Console.WriteLine("Import Id = {0}", importResult.Id);
            Console.WriteLine("Import Status = {0}", importResult.Status);
        }

        private static void PrintContactImportReport(ApiContactImportReport contactImportReport)
        {
            Console.WriteLine();
            Console.WriteLine("{0, -20} = {1}", "NewContacts", contactImportReport.NewContacts);
            Console.WriteLine("{0, -20} = {1}", "UpdatedContacts", contactImportReport.UpdatedContacts);
        }

        private static string GetContactImportFaults(Guid importId)
        {
            byte[] csvData = _client.GetContactImportReportFaults(importId);
            using (MemoryStream memoryStream = new MemoryStream(csvData))
            {
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static void PrintCsvReport(string csvReport)
        {
            Console.WriteLine();
            Console.WriteLine(csvReport);
        }
    }
}
