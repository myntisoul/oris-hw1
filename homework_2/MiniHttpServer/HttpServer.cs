using System.Net;
namespace oris_hw1;
public sealed class HttpServer
{
    private HttpListener? _listener;
    public void Start()
    {
        var settings = oris_hw1.framework.core.ConfigurationManager.Load();
        _listener = new HttpListener();
        foreach (var prefix in settings.Prefixes)
            _listener.Prefixes.Add(prefix);
        _listener.Start();
        _ = RunAsync(_listener);
        Console.WriteLine("Сервер запущен");
    }
    public void Stop()
    {
        _listener?.Stop();
        _listener?.Close();
    }
    private static async Task RunAsync(HttpListener listener)
    {
        while (listener.IsListening)
        {
            HttpListenerContext context;
            try { context = await listener.GetContextAsync(); }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }
            _ = ProcessAsync(context);
        }
    }
    private static async Task ProcessAsync(HttpListenerContext context)
    {
        try
        {
            string relative = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/").TrimStart('/');
            if (string.IsNullOrEmpty(relative)) relative = "index.html";
            var basePath = AppContext.BaseDirectory;
            var path = Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!path.StartsWith(Path.GetFullPath(basePath), StringComparison.Ordinal))
            {
                context.Response.StatusCode = 403;
                return;
            }
            if (!File.Exists(path))
            {
                context.Response.StatusCode = 404;
                return;
            }
            var ext = Path.GetExtension(path).ToLowerInvariant();
            context.Response.ContentType = ext switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".svg" => "image/svg+xml",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                _ => "application/octet-stream"
            };
            context.Response.ContentLength64 = new FileInfo(path).Length;
            await using var stream = File.OpenRead(path);
            await stream.CopyToAsync(context.Response.OutputStream);
        }
        catch (Exception)
        {
            try { context.Response.StatusCode = 500; } catch { }
        }
        finally { try { context.Response.Close(); } catch { } }
    }
}
