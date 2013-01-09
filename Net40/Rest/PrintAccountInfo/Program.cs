using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace dotMailer.PrintAccountInfo
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            HttpClient client = GetHttpClient();

            HttpResponseMessage response = client.GetAsync("/v2/account-info").Result;
            ApiAccount account = response.Content.ReadAsAsync<ApiAccount>().Result;

            PrintResult(account);
        }

        private static void PrintResult(ApiAccount account)
        {
            Console.WriteLine("Account ID - " + account.ID);
            foreach (ApiAccountProperty property in account.Properties)
            {
                Console.WriteLine("{0} {1} = {2}", property.Type, property.Name, property.Value);
            }
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
