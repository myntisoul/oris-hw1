using System.Net;
namespace oris_hw1.framework.core;
public sealed class ControllersHandler : RequestHandler
{
    private readonly HttpServer _server;
    public ControllersHandler(HttpServer server) { _server = server; }
    public override async Task<bool> HandleAsync(HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var result = await _server.TryRoute(context.Request.HttpMethod, context.Request.Url?.AbsolutePath ?? "/", body, context.Request.QueryString);
        if (!result.isHandled) return await base.HandleAsync(context);
        context.Response.StatusCode = result.code;
        var bytes = System.Text.Encoding.UTF8.GetBytes(result.responseHtml);
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        return true;
    }
}
