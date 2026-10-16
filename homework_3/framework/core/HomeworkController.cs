using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace oris_hw1.framework.core
{
    [HttpController("home-work")]
    class HomeworkController
    {
        [HttpGET]
        public async Task Send(string to)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("артём каримов", "matje_bal1@mail.ru"));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = "домашко 11-541 каримов артём маратович";

                var builder = new BodyBuilder();
                builder.HtmlBody = "<h2>домашко 11-541 каримов артём маратович</h2>";

                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "homework.zip");
                if (File.Exists(zipPath))
                {
                    builder.Attachments.Add(zipPath);
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync("smtp.mail.ru", 465, true);
                await client.AuthenticateAsync("matje_bal1@mail.ru", "uUNgpYstL5EybUo1Wm9k");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine("Письмо отправлено!");
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
    }
}