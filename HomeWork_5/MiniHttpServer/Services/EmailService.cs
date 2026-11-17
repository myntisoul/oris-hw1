using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject1;
using MailKit;
using MailKit.Net.Smtp;

namespace oris_hw1.framework.core
{
    public class RealEmailSender: IEmailSender, IEmailService
    {
        public Task SendAsync(string recipient) => Send(recipient);

        public async Task Send(string to)
        {
            try
            {
                string? smtpUser = Environment.GetEnvironmentVariable("ORIS_SMTP_USER");
                string? smtpPassword = Environment.GetEnvironmentVariable("ORIS_SMTP_PASSWORD");
                if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
                    throw new InvalidOperationException("Укажите ORIS_SMTP_USER и ORIS_SMTP_PASSWORD.");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ORIS", smtpUser));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = "домашко 11-541 каримов артём маратович";

                var builder = new BodyBuilder();
                builder.HtmlBody = "<h2>домашко 11-541 каримов артём маратович</h2>";

                string zipPath = Path.Combine(AppContext.BaseDirectory, "homework.zip");
                if (File.Exists(zipPath))
                {
                    builder.Attachments.Add(zipPath);
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync("smtp.mail.ru", 465, true);
                await client.AuthenticateAsync(smtpUser, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
    }
}
