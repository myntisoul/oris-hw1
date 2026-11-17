using oris_hw1;
using oris_hw1.framework.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class FakeEmailSender: IEmailSender
    {
        public bool HasBeenSent { get; set; } = false;
        public string Recepient {  get; set; }

        public Task Send(string to)
        {
            Recepient = to;
            HasBeenSent = true;
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class HomeworkControllerTest
    {
        [TestMethod]
        public async Task Send_ShouldCallSend()
        {
            var fakeSender = new FakeEmailSender();
            var controller = new HomeworkController(fakeSender);

            await controller.Send("test@test.com");
            Assert.IsTrue(fakeSender.HasBeenSent);
            Assert.AreEqual(fakeSender.Recepient, "test@test.com");
        }
    }
}
