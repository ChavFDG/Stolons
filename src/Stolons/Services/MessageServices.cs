using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Stolons.Services
{
    public static class AuthMessageSender 
    {

        /// <summary>
        /// MAIL KIT
        /// Info : http://dotnetthoughts.net/how-to-send-emails-from-aspnet-core/
        /// </summary>
        public static void SendEmail(string email, string name, string subject, string message,byte[] attachment = null,string attachmentName ="Facture")
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(Configurations.ApplicationConfig.StolonsLabel, Configurations.ApplicationConfig.MailAddress));
            mimeMessage.To.Add(new MailboxAddress(name, email));
            mimeMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder();
            if(attachment != null)
                bodyBuilder.Attachments.Add(attachmentName,attachment);
            bodyBuilder.HtmlBody = message;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(Configurations.ApplicationConfig.MailSmtp, Configurations.ApplicationConfig.MailPort, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    // Note: since we don't have an OAuth2 token, disable 	
                    // the XOAUTH2 authentication mechanism.     
                    client.Authenticate(Configurations.ApplicationConfig.MailAddress, Configurations.ApplicationConfig.MailPassword);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }

            }
            catch(Exception except)
            {
                Console.WriteLine("Error on sending mail : " + except.Message);
            }
        }

        public static Task SendSmsAsync(string number, string message)
        {


            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }   
}
