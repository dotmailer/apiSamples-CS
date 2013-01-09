using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace dotMailer.PrintCampaignsWithActivity
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

            DateTime firstDayOfMonth = GetFirstDayOfMonth();
            IEnumerable<ApiCampaign> campaigns = GetAllCampaignWithActivity(firstDayOfMonth);

            PrintCampaigns(campaigns);
        }

        private static IEnumerable<ApiCampaign> GetAllCampaignWithActivity(DateTime startDate)
        {
            const int select = 1000;
            int skip = 0;

            IEnumerable<ApiCampaign> campaigns;
            do
            {
                String url = String.Format("/v2/campaigns/with-activity-since/{0:s}?select={1}&skip={2}", startDate, select, skip);
                HttpResponseMessage response = _client.GetAsync(url).Result;
                campaigns = response.Content.ReadAsAsync<IEnumerable<ApiCampaign>>().Result;

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

        private static DateTime GetFirstDayOfMonth()
        {
            DateTime currentTime = DateTime.Now;
            DateTime result = new DateTime(currentTime.Year, currentTime.Month, 1);
            
            return result;
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
