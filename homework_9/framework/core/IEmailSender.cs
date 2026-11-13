using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public interface IEmailSender
    {
        public Task Send(string to);
    }
}
