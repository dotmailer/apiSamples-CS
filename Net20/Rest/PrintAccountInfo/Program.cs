using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace dotMailer.PrintAccountInfo
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            WebRequest request = CreateRequest("https://apiconnector.com/v2/account-info");
            ApiAccount account = ReadResult<ApiAccount>(request);

            PrintResult(account);
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

        private static void PrintResult(ApiAccount account)
        {
            Console.WriteLine("Account ID - " + account.ID);
            foreach (ApiAccountProperty property in account.Properties)
            {
                Console.WriteLine("{0} {1} = {2}", property.Type, property.Name, property.Value);
            }
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
