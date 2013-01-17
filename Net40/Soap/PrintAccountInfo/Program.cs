using System;
using dotMailer.PrintAccountInfo.ApiServiceReference;
using System.ServiceModel;
namespace dotMailer.PrintAccountInfo
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            ApiServiceClient client = GetWcfClient();

            ApiAccount account = client.GetCurrentAccountInfo();

            PrintResult(account);
        }

        private static void PrintResult(ApiAccount account)
        {
            Console.WriteLine("Account ID - " + account.Id);
            foreach (ApiAccountProperty property in account.Properties)
            {
                Console.WriteLine("{0} {1} = {2}", property.Type, property.Name, property.Value);
            }
        }

        private static ApiServiceClient GetWcfClient()
        {
            ApiServiceClient client = new ApiServiceClient("Secure_ApiService");
            client.ClientCredentials.UserName.UserName = UserName;
            client.ClientCredentials.UserName.Password = Password;
            return client;
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
