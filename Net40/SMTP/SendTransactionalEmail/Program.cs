using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SendTransactionalEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            // credentials
            var smtpHostname = "r1-smtp.dotmailer.com";
            var transactionalApiUsername = "your-transactional-user@apiconnector.com";
            var transactionalApiPassword = "your-password";

            using (var client = new SmtpClient(smtpHostname))
            {
                client.Credentials = new NetworkCredential(transactionalApiUsername, transactionalApiPassword);
                client.EnableSsl = true;
                using (var msg = new MailMessage("from@yourcompany.com", "youraddress@youraddress.com"))
                {
                    msg.Subject = "Hello world";
                    msg.Body = "<b>Hello</b> from dotmailer transactional email";
                    msg.IsBodyHtml = true;
                    client.Send(msg);
                }
            }

        }
    }
}
