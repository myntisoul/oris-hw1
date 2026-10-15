using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
//using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;

namespace NetConsoleApp
{
    class SendMessage
    {


        public async Task Send()
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("artem", "matje_bal1@mail.ru"));
                message.To.Add(new MailboxAddress("", "artem_mkarimov@mail.ru"));
                message.Subject = "Тест";
                message.Body = new TextPart("html") { Text = "<h2>Письмо-тест</h2>" };

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