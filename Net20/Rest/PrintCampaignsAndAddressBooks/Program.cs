using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace dotMailer.PrintCampaignsAndAddressBooks
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            PrintAllCampaigns();
            PrintEmptyLine();
            PrintAllAddressBooks();
        }

        private static void PrintAllAddressBooks()
        {
            PrintTableName("Address Books");
            PrintAddressBookRow("Id", "Name");

            DataReader<ApiAddressBook> addressBooks = new DataReader<ApiAddressBook>(GetAddressBooksPaged);
            foreach (ApiAddressBook addressBook in addressBooks.ReadAll())
            {
                PrintAddressBookRow(addressBook.Id.ToString(), addressBook.Name);
            }
        }

        private static IEnumerable<ApiAddressBook> GetAddressBooksPaged(int @select, int skip)
        {
            String url = String.Format("https://apiconnector.com/v2/address-books?select={0}&skip={1}", select, skip);
            WebRequest request = CreateRequest(url);
            IEnumerable<ApiAddressBook> result = ReadResult<IEnumerable<ApiAddressBook>>(request);
            return result;
        }

        private static void PrintEmptyLine()
        {
            Console.WriteLine();
        }

        private static void PrintAllCampaigns()
        {
            PrintTableName("Campaigns");
            PrintCampaignTableRow("Id", "Name", "Subject", "FromName");

            DataReader<ApiCampaign> campaigns = new DataReader<ApiCampaign>(GetCampaignsPaged);
            foreach (ApiCampaign campaign in campaigns.ReadAll())
            {
                PrintCampaignTableRow(campaign.Id.ToString(), campaign.Name, campaign.Subject, campaign.FromName);
            }
        }

        private static IEnumerable<ApiCampaign> GetCampaignsPaged(int select, int skip)
        {
            String url = String.Format("https://apiconnector.com/v2/campaigns?select={0}&skip={1}", select, skip);
            WebRequest request = CreateRequest(url);
            IEnumerable<ApiCampaign> result = ReadResult<IEnumerable<ApiCampaign>>(request);
            return result;
        }

        private static void PrintTableName(string tableName)
        {
            Console.WriteLine("{0, 40}\n", tableName);
        }

        private static void PrintCampaignTableRow(string idRow, string nameColumn, string subjectColumn, string fromNameColumn)
        {
            Console.WriteLine("{0,-10} {1,-25} {2,-20} {3,-20}", idRow, nameColumn, subjectColumn, fromNameColumn);
        }

        private static void PrintAddressBookRow(string idRow, string nameColumn)
        {
            Console.WriteLine("{0,-10} {1,-55}", idRow, nameColumn);
        }

        private static WebRequest CreateRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);
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
