using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace dotMailer.PrintCampaignsWithActivity
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            DateTime firstDayOfMonth = GetFirstDayOfMonth();
            IEnumerable<ApiCampaign> campaigns = GetAllCampaignWithActivity(firstDayOfMonth);

            PrintCampaigns(campaigns);
        }

        private static IEnumerable<ApiCampaign> GetAllCampaignWithActivity(DateTime startDate)
        {
            const int select = 1000;
            int skip = 0;

            bool shouldLoadMore;
            do
            {
                String url = String.Format("https://apiconnector.com/v2/campaigns/with-activity-since/{0:s}?select={1}&skip={2}", startDate, select, skip);
                WebRequest request = CreateRequest(url);
                IEnumerable<ApiCampaign> items = ReadResult<IEnumerable<ApiCampaign>>(request);
                List<ApiCampaign> campaigns = new List<ApiCampaign>(items);

                foreach (ApiCampaign campaign in campaigns)
                {
                    yield return campaign;
                }

                skip += select;

                shouldLoadMore = (select == campaigns.Count);
            } while (shouldLoadMore);
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
