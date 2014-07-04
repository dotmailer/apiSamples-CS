using System;
using dotMailer.CreateAccount.AccountsApiServiceReference;
using System.ServiceModel;

namespace dotMailer.CreateAccount
{
    class Program
    {
        private const string UserName = "{ Your username here }";
        private const string Password = "{ Your password here }";

        private static AccountsApiServiceClient _client;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledException;

            _client = GetWcfClient();

            ApiAccount account = CreateAccount();

            UpdateTheme(account);

            CreateManagedUser(account);

            CreateApiUser(account);

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static AccountsApiServiceClient GetWcfClient()
        {
            AccountsApiServiceClient client = new AccountsApiServiceClient("Secure_AccountsApiService");
            client.ClientCredentials.UserName.UserName = UserName;
            client.ClientCredentials.UserName.Password = Password;
            return client;
        }

        private static ApiAccount CreateAccount()
        {
            string emailAddress = GenerateEmailAddress();
            string password = GeneratePassword();

            ApiAccount account = new ApiAccount
            {
                Properties = new ApiAccountProperty[]
                {
                    //These fields are mandatory
                    new ApiAccountProperty
                    {
                        Name = "EMAIL",
                        Value = emailAddress,
                    },
                    new ApiAccountProperty
                    {
                        Name = "FIRSTNAME",
                        Value = "firstname",
                    },
                    new ApiAccountProperty
                    {
                        Name = "LASTNAME",
                        Value = "lastname",
                    },
                    new ApiAccountProperty
                    {
                        Name = "PASSWORD",
                        Value = password, 
                    },
                    new ApiAccountProperty
                    {
                        Name = "USERIPADDRESS",
                        //should reflect the IP address that the account holder is likely to log in from.
                        Value = "127.0.0.1" 
                    },

                    //The following fields are optional
                    new ApiAccountProperty
                    {
                        Name = "COMPANY",
                        Value = "a-company",
                    },
                    new ApiAccountProperty
                    {
                        Name = "PHONE",
                        Value = "02080001000",
                    },
                    new ApiAccountProperty
                    {
                        //you can find a list of locale ids @ http://msdn.microsoft.com/en-us/library/ms912047(v=winembedded.10).aspx
                        Name = "CULTUREID",
                        Value = "2057", //en-GB
                    },
                    new ApiAccountProperty
                    {
                        Name = "TIMEZONEID",
                        //you can find a list of timezone ids @ http://msdn.microsoft.com/en-us/library/gg154758.aspx
                        Value = "GMT Standard Time",
                    },
                    new ApiAccountProperty
                    {
                        Name = "onlinepaymentsenabled",
                        Value = "false"
                    }
                }
            };

            ApiAccount createdAccount = _client.CreateAccount(account);
            Console.WriteLine("Account '{0}' has been created with the password '{1}' and assigned the ID '{2}'", emailAddress, password, createdAccount.Id);

            return createdAccount;
        }

        private static void UpdateTheme(ApiAccount forAccount)
        {
            //example colours taken from: http://paletton.com/#uid=7280u0kllllaFw0g0qFqFg0w0aF
            ApiTheme theme = new ApiTheme
            {
                ButtonColor = "#71266e",
                HyperlinkColor = "#993350",
                PrimaryColor = "#90a437",
                SecondaryColor = "#5e9732"
            };

            _client.UpdateTheme(forAccount.Id, theme);

            Console.WriteLine("Theme succesfully updated for account");
        }

        private static void CreateManagedUser(ApiAccount forAccount)
        {
            string password = GeneratePassword();
            ApiManagedUser managedUser = new ApiManagedUser
            {
                Email = GenerateEmailAddress(),
                FirstName = "Firstname",
                LastName = "Lastname",
                MobileNumber = "07511111111",
                Password = password,
                //The list of permissions you want to assign to a managed user.
                Permissions = new ApiPermissionTypes[]
                {
                    ApiPermissionTypes.ContactExporter,
                    ApiPermissionTypes.Contacts,
                    ApiPermissionTypes.EditCampaigns,
                    ApiPermissionTypes.Programs,
                    ApiPermissionTypes.ReportDrillDown, 
                    ApiPermissionTypes.Reporter,
                    ApiPermissionTypes.Sender,
                    ApiPermissionTypes.TemplateAdmin,
                    ApiPermissionTypes.ViewAccountInvoices
                }
            };

            ApiManagedUser createdManagedUser = _client.CreateManagedUser(forAccount.Id, managedUser);
            Console.WriteLine("Managed user '{0}' has been created with password '{1}'", createdManagedUser.Email, password);
        }

        private static void CreateApiUser(ApiAccount forAccount)
        {
            string password = GeneratePassword();
            ApiUser apiUser = new ApiUser
            {
                Email = GenerateEmailAddress(),
                Password = password,
            };

            ApiUser createdApiUser = _client.CreateApiUser(forAccount.Id, apiUser);
            Console.WriteLine("API user '{0}' has been created with the password '{1}'", createdApiUser.Email, password);
        }

        private static void PrintUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        /// <summary>
        /// Generates a password. This password should only be used for demo purposes and can not be considered secure.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// All passwords must conform to the dotMailer password policy: 
        /// - they must be at least 8 characters in length
        /// - they must contain 2 or more non-alpha characters
        /// </remarks>
        public static string GeneratePassword()
        {
            return "temporaryPw-" + Guid.NewGuid().ToString();
        }

        public static string GenerateEmailAddress()
        {
            return string.Format("email-{0}@test-domain.com", Guid.NewGuid());
        }
    }
}
