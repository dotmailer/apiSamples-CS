using System;
using System.Collections.Generic;
using System.Net;
using dotMailer.PrintCampaignsAndAddressBooks.dotMailer;

namespace dotMailer.PrintCampaignsAndAddressBooks
{
    internal class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static ApiService _client;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetSoapClient();

            PrintAllCampaigns();
            PrintEmptyLine();
            PrintAllAddressBooks();
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
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

        private static void PrintTableName(string tableName)
        {
            Console.WriteLine("{0, 40}\n", tableName);
        }

        private static void PrintCampaignTableRow(string idRow, string nameColumn, string subjectColumn, string fromNameColumn)
        {
            Console.WriteLine("{0,-10} {1,-25} {2,-20} {3,-20}", idRow, nameColumn, subjectColumn, fromNameColumn);
        }

        private static IEnumerable<ApiCampaign> GetCampaignsPaged(int select, int skip)
        {
            return _client.GetCampaigns(select, true, skip, true);
        }

        private static void PrintEmptyLine()
        {
            Console.WriteLine();
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

        private static void PrintAddressBookRow(string idRow, string nameColumn)
        {
            Console.WriteLine("{0,-10} {1,-55}", idRow, nameColumn);
        }

        private static ApiService GetSoapClient()
        {
            ApiService client = new ApiService();
            client.Credentials = new NetworkCredential(UserName, Password);
            return client;
        }

        private static IEnumerable<ApiAddressBook> GetAddressBooksPaged(int select, int skip)
        {
            return _client.GetAddressBooks(select, true, skip, true);
        }
    }
}