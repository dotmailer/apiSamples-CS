using System;
using System.IO;
using System.Net;
using System.Threading;
using dotMailer.ImportContacts.dotMailer;

namespace dotMailer.ImportContacts
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";
        private const int AddressBookId = 0; //Your address book here

        private static Secure_ApiService _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetSoapClient();

            byte[] contacts = GetContactsForImport();
            ApiContactImport contactImport = _client.ImportContactsToAddressBook(AddressBookId, true, contacts, "csv");
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

        private static string GetContactImportFaults(String importId)
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

        private static Secure_ApiService GetSoapClient()
        {
            Secure_ApiService client = new Secure_ApiService();
            client.Credentials = new NetworkCredential(UserName, Password);
            return client;
        }

        private static void PrintCsvReport(string csvReport)
        {
            Console.WriteLine();
            Console.WriteLine(csvReport);
        }
    }
}
