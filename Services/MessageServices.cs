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
using Stolons.Models;
using Microsoft.Extensions.Logging; 
using Stolons.Helpers;

namespace Stolons.Services
{
    public static class AuthMessageSender
    {

        /// <summary>
        /// MAIL KIT
        /// Info : http://dotnetthoughts.net/how-to-send-emails-from-aspnet-core/
        /// </summary>
        public static void SendEmail(string senderLabel, string email, string name, string subject, string message, byte[] attachment = null, string attachmentName="Facture")
        {
            if (String.IsNullOrWhiteSpace(name))
                name = email;
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(senderLabel, Configurations.Application.MailAddress));
            mimeMessage.To.Add(new MailboxAddress(name, email));
            mimeMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder();
            if (attachment != null)
                bodyBuilder.Attachments.Add(attachmentName, attachment);
            bodyBuilder.HtmlBody = message;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            try
            {
                using (var client = new SmtpClient())
                {
		    if (Configurations.Environment.EnvironmentName == "Debug" || Configurations.Environment.EnvironmentName == "Development")
		    {
			client.Connect(Configurations.DebugMailSmtp, Configurations.DebugMailPort, false);
			client.AuthenticationMechanisms.Remove("XOAUTH2");
			// Note: since we don't have an OAuth2 token, disable
			// the XOAUTH2 authentication mechanism.
			if (Configurations.DebugMailUser != null && Configurations.DebugMailPassword != null)
			    client.Authenticate(Configurations.DebugMailUser, Configurations.DebugMailPassword);
		    }
		    else
		    {
			client.Connect(Configurations.Application.MailSmtp, Configurations.Application.MailPort, false);
			client.AuthenticationMechanisms.Remove("XOAUTH2");
			// Note: since we don't have an OAuth2 token, disable
			// the XOAUTH2 authentication mechanism.
			client.Authenticate(Configurations.Application.MailAddress, Configurations.Application.MailPassword);
		    }
		    client.Send(mimeMessage);
		    client.Disconnect(true);
                }
            }
	    catch (Exception except)
            {
                DotnetHelper.GetLogger<String>().LogError("Error on sending mail : " + except.Message);
            }
        }

        public static Task SendSmsAsync(string number, string message)
        {
	    // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
