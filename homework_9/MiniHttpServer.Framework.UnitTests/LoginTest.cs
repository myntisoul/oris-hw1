using oris_hw1.framework.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public sealed class LoginTest
    {
        [TestMethod]
        public void LoginTestReturnHTML()
        {
            var controller = new LoginController();
            string result = controller.Auth("test@gmail.com", "qfgaegwgegeage");
            Assert.AreEqual("<h1>Успешно! Данные отправлены.</h1><a href='/Login'>Назад</a>", result);
        }
    }
}
