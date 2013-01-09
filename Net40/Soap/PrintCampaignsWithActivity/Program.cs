using System;
using System.Collections.Generic;
using System.Linq;
using dotMailer.PrintCampaignsWithActivity.ApiServiceReference;

namespace dotMailer.PrintCampaignsWithActivity
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static ApiServiceClient _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetWcfClient();

            DateTime firstDayOfMonth = GetFirstDayOfMonth();
            IEnumerable<ApiCampaign> campaigns = GetAllCampaignWithActivity(firstDayOfMonth);

            PrintCampaigns(campaigns);
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

            IEnumerable<ApiCampaign> campaigns;
            do
            {
                campaigns = _client.GetSentCampaignsWithActivitySinceDate(startDate, select, skip);

                foreach (ApiCampaign campaign in campaigns)
                {
                    yield return campaign;
                }

                skip += select;
            } while (campaigns.Any());
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
