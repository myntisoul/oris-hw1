using System.Net;
namespace oris_hw1.framework.core;
public sealed class RequestContext
{
    public HttpListenerContext HttpContext { get; }
    public RequestContext(HttpListenerContext context) { HttpContext = context; }
}
