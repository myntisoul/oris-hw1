using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oris_hw1.framework.core
{
    [HttpController]
    public class LoginController
    {
        [HttpGET]
        public string Index()
        {
            return System.IO.File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "/static/login.html");
        }

        [HttpPOST]
        public string Auth(string email, string password)
        {
            Console.WriteLine($"email: {email}");
            Console.WriteLine($"password: {password}");
            return "<h1>Успешно! Данные отправлены.</h1><a href='/Login'>Назад</a>";
        }
    }
}
