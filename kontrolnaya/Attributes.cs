using System;

namespace oris_hw1.kontrolnaya
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpController : Attribute
    {
        public string Name { get; }
        public HttpController(string name = null) { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGET : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPOST : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPUT : Attribute { }
}