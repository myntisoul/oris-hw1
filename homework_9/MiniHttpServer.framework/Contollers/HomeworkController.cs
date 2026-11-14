using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject1;

namespace oris_hw1.framework.core
{
    [HttpController("home-work")]
    public class HomeworkController
    {
        private readonly IEmailSender _emailSender;

        public HomeworkController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGET]
        public async Task<string> Send(string to)
        {
            await _emailSender.Send(to);
            return "<h1>Отправлено</h1>";
        }
    }
}
