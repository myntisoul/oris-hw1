using MimeKit.Cryptography;
using Org.BouncyCastle.Tls;
using oris_hw1.framework.core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public class TryRouteTest
    {
        [TestMethod]

        public async Task SendEmailTest_TryRoute_FakeMailSender()
        {
            var server = new HttpServer();
            var fakeSender = new FakeEmailSender();
            server.EmailSender = fakeSender;
            var queryString = new NameValueCollection() { { "to", "test@mail.ru" } };
            var result = await server.TryRoute("GET", "/home-work/send", "", queryString);
            Assert.IsTrue(result.isHandled);
            Assert.IsTrue(fakeSender.HasBeenSent);
        }
    }
}
