using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Mail;

namespace NetConsoleApp
{
    class SendMessage
    {
        public void Send()
        {
            //GET http://localhost:2323/steam/send?to=test@test.ru&hero=

            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("matje_bal1@mail.ru", "artem");
            // кому отправляем
            MailAddress toAddress = new MailAddress("artem_mkarimov@mail.ru");
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, toAddress);
            // тема письма
            m.Subject = "Тест";
            // текст письма
            m.Body = "<h2>Письмо-тест работы smtp-клиента</h2>";
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.mail.ru", 465);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("matje_bal1@mail.ru", "uUNgpYstL5EybUo1Wm9k");
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(m);
                Console.WriteLine("sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}