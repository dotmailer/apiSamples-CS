using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace dotMailer.PrintCampaignsAndAddressBooks
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

            PrintAllCampaigns();
            PrintEmptyLine();
            PrintAllAddressBooks();
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

        private static void PrintTableName(string tableName)
        {
            Console.WriteLine("{0, 40}\n", tableName);
        }

        private static IEnumerable<ApiAddressBook> GetAddressBooksPaged(int @select, int skip)
        {
            String url = String.Format("/v2/address-books?select={0}&skip={1}", select, skip);
            HttpResponseMessage response = _client.GetAsync(url).Result;
            IEnumerable<ApiAddressBook> result = response.Content.ReadAsAsync<IEnumerable<ApiAddressBook>>().Result;

            return result;
        }

        private static IEnumerable<ApiCampaign> GetCampaignsPaged(int select, int skip)
        {
            String url = String.Format("/v2/campaigns?select={0}&skip={1}", select, skip);
            HttpResponseMessage response = _client.GetAsync(url).Result;
            IEnumerable<ApiCampaign> result = response.Content.ReadAsAsync<IEnumerable<ApiCampaign>>().Result;

            return result;
        }

        private static void PrintCampaignTableRow(string idRow, string nameColumn, string subjectColumn, string fromNameColumn)
        {
            Console.WriteLine("{0,-10} {1,-25} {2,-20} {3,-20}", idRow, nameColumn, subjectColumn, fromNameColumn);
        }

        private static void PrintAddressBookRow(string idRow, string nameColumn)
        {
            Console.WriteLine("{0,-10} {1,-55}", idRow, nameColumn);
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
