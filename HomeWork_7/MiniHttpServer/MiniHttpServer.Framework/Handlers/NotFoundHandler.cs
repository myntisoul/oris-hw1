using System.Net;
namespace oris_hw1.framework.core;
public sealed class NotFoundHandler : RequestHandler
{
    public override async Task<bool> HandleAsync(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        var path = Path.Combine(AppContext.BaseDirectory, "static", "404.html");
        var data = File.Exists(path) ? await File.ReadAllBytesAsync(path) : System.Text.Encoding.UTF8.GetBytes("Not Found");
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = data.Length;
        await context.Response.OutputStream.WriteAsync(data);
        return true;
    }
}
