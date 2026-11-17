using System.Net;
namespace oris_hw1.framework.core;
public abstract class RequestHandler
{
    protected RequestHandler? Next { get; private set; }
    public RequestHandler SetNext(RequestHandler handler)
    {
        Next = handler;
        return handler;
    }
    public virtual async Task<bool> HandleAsync(HttpListenerContext context)
    {
        return Next != null && await Next.HandleAsync(context);
    }
}
