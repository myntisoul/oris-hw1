using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oris_hw1.framework.core
{
    public class LoginController
    {
        [HttpGet]
        public string Index()
        {
            return System.IO.File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "/static/login.html");
        }

        [HttpPost]
        public string Auth(string username, string password)
        {
            Console.WriteLine($"username: {username}");
            Console.WriteLine($"password: {password}");
            return "<h1>Успешно! Данные отправлены.</h1><a href='/Login'>Назад</a>";
        }
    }
}
