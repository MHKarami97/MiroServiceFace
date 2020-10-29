using System;
using MimeKit;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;

        public EmailSender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMesage = CreateEmailMessage(message);
            await SendAsync(emailMesage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
                {HtmlBody = $"<h2 style='color:red;'>{message.Content}</h2>"};

            if (message.Attachments != null && message.Attachments.Any())
            {
                var i = 1;

                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add("attachment" + i, attachment);

                    i++;
                }
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }

        private async Task SendAsync(MimeMessage emailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);

                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    //This needs to be logged actually
                    await Console.Out.WriteLineAsync("" + ex.Message);

                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}