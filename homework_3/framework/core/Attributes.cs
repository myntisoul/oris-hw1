using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oris_hw1.framework.core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpController : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGet : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPost : Attribute { }
}
