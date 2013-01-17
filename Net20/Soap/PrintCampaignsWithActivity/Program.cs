using System;
using System.Collections.Generic;
using System.Net;
using dotMailer.PrintCampaignsWithActivity.dotMailer;

namespace dotMailer.PrintCampaignsWithActivity
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static ApiService _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetSoapClient();

            DateTime firstDayOfMonth = GetFirstDayOfMonth();
            IEnumerable<ApiCampaign> campaigns = GetAllCampaignWithActivity(firstDayOfMonth);

            PrintCampaigns(campaigns);
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private static DateTime GetFirstDayOfMonth()
        {
            DateTime currentTime = DateTime.Now;
            DateTime result = new DateTime(currentTime.Year, currentTime.Month, 1);

            return result;
        }

        private static IEnumerable<ApiCampaign> GetAllCampaignWithActivity(DateTime startDate)
        {
            const int select = 1000;
            int skip = 0;

            bool shouldLoadMore;
            do
            {
                IEnumerable<ApiCampaign> items = _client.GetSentCampaignsWithActivitySinceDate(startDate, true, select, true, skip, true);
                List<ApiCampaign> campaigns = new List<ApiCampaign>(items);
                foreach (ApiCampaign campaign in campaigns)
                {
                    yield return campaign;
                }

                skip += select;

                shouldLoadMore = (select == campaigns.Count);
            } while (shouldLoadMore);
        }

        private static ApiService GetSoapClient()
        {
            ApiService client = new ApiService();
            client.Credentials = new NetworkCredential(UserName, Password);
            return client;
        }

        private static void PrintCampaigns(IEnumerable<ApiCampaign> campaigns)
        {
            foreach (ApiCampaign campaign in campaigns)
            {
                Console.WriteLine("{0}", campaign.Name);
            }
        }
    }
}
