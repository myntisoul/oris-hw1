using oris_hw1.framework.core;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Xml.Linq;

namespace oris_hw1.framework.core;

public class HttpServer
{
    private int _port = 8888;
    private string filePath = Path.Combine(AppContext.BaseDirectory, "static", "index.html");
    private HttpListener _listener;

    public void Start()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Ошибка: файл {filePath} не найден!");
            return;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://127.0.0.1:" + _port.ToString() + "/");
        _listener.Start();
        Console.WriteLine("Сервер начал свою работу");
        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Сервер завершил свою работу");
    }

    private void Receive()
    {
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        HttpListenerContext context = null;
        try
        {
            if (_listener.IsListening)
            {
                context = _listener.EndGetContext(result);
                var request = context.Request;
                string path = request.Url?.LocalPath ?? "/";
                var response = context.Response;

                if (string.IsNullOrEmpty(path) || path.EndsWith("/"))
                {
                    path += "index.html";
                }

                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();
                string httpMethod = context.Request.HttpMethod;
                NameValueCollection queryString = request.QueryString;

                var (isHandledByController, responseHtml, statusCode) = await TryRoute(httpMethod, path, body, queryString);

                if (isHandledByController)
                {
                    context.Response.StatusCode = statusCode;

                    if (statusCode == 303)
                    {
                        context.Response.AddHeader("Location", "/users/index");
                    }

                    if (!string.IsNullOrEmpty(responseHtml))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentType = "text/html; charset=utf-8";

                        await context.Response.OutputStream.WriteAsync(buffer);
                        await context.Response.OutputStream.FlushAsync();
                    }
                    else
                    {
                        context.Response.ContentLength64 = 0;
                    }

                    context.Response.Close();
                    Receive();
                    return;
                }

                string staticFilePath = Path.Combine(AppContext.BaseDirectory, "static", path.TrimStart('/'));
                FileInfo fileInfo = new FileInfo(staticFilePath);

                if (!fileInfo.Exists)
                {
                    response.StatusCode = 404;
                    staticFilePath = Path.Combine(AppContext.BaseDirectory, "static", "404.html");
                    fileInfo = new FileInfo(staticFilePath);
                }

                switch (fileInfo.Extension.ToLower())
                {
                    case ".html": response.ContentType = "text/html; charset=utf-8"; break;
                    case ".css": response.ContentType = "text/css; charset=utf-8"; break;
                    case ".js": response.ContentType = "text/javascript; charset=utf-8"; break;
                    case ".png": response.ContentType = "image/png"; break;
                    case ".ico": response.ContentType = "image/x-icon"; break;
                    case ".svg": response.ContentType = "image/svg+xml"; break;
                    case ".jpg": response.ContentType = "image/jpeg"; break;
                }

                if (fileInfo.Exists)
                {
                    byte[] buffer = await File.ReadAllBytesAsync(staticFilePath);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                    await response.OutputStream.FlushAsync();
                }

                context.Response.Close();
                Receive();
            }
        }
        catch (HttpListenerException)
        {
            Console.WriteLine("stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            try
            {
                if (context != null && context.Response.OutputStream.CanWrite)
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
            catch { }
            Receive();
        }
    }

    public TestProject1.IEmailSender? EmailSender { get; set; }

    public async Task<(bool isHandled, string responseHtml, int code)> TryRoute(string httpMethod, string path, string body, NameValueCollection queryString)
    {
        if (path.Contains('.') || path == "/") return (false, "", 200);
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return (false, "", 200);
        var controllerName = segments[0];
        var actionName = segments.Length > 1 ? segments[1] : "index";
        var controller = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try { return a.GetExportedTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.OfType<Type>(); }
            })
            .FirstOrDefault(t => t.IsClass && !t.IsAbstract &&
                t.GetCustomAttribute<oris_hw1.framework.core.HttpController>() is { } attribute &&
                string.Equals(attribute.Name ?? t.Name.Replace("Controller", ""), controllerName, StringComparison.OrdinalIgnoreCase));
        if (controller == null) return (false, "", 200);
        var method = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => string.Equals(m.Name, actionName, StringComparison.OrdinalIgnoreCase) &&
                m.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "Http" + httpMethod, StringComparison.OrdinalIgnoreCase)));
        if (method == null) return (false, "", 200);
        var form = new NameValueCollection();
        if (httpMethod is "POST" or "PUT" or "PATCH")
        {
            foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2)
                    form.Add(Uri.UnescapeDataString(kv[0].Replace("+", " ")), Uri.UnescapeDataString(kv[1].Replace("+", " ")));
            }
        }
        var arguments = method.GetParameters().Select(p =>
        {
            if (p.ParameterType == typeof(NameValueCollection))
                return (object)(string.Equals(p.Name, "query", StringComparison.OrdinalIgnoreCase) ? queryString : form);
            string? value = (httpMethod == "GET" ? queryString : form)[p.Name] ?? queryString[p.Name];
            if (p.Name == "body" && p.ParameterType == typeof(string)) return (object?)body;
            if (value == null) return p.HasDefaultValue ? p.DefaultValue :
                (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null);
            return Convert.ChangeType(value, Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType);
        }).ToArray();
        var ctor = controller.GetConstructors()
            .OrderBy(c => c.GetParameters().Length)
            .FirstOrDefault();
        if (ctor == null) return (false, "", 500);
        var ctorArgs = ctor.GetParameters().Select(p =>
            p.ParameterType == typeof(TestProject1.IEmailSender) ? (object?)EmailSender :
            (p.HasDefaultValue ? p.DefaultValue : null)).ToArray();
        var instance = ctor.Invoke(ctorArgs);
        var value = method.Invoke(instance, arguments);
        if (value is Task task)
        {
            await task;
            value = task.GetType().GetProperty("Result")?.GetValue(task);
        }
        if (value is ValueTuple<string, int> response)
            return (true, response.Item1, response.Item2);
        return (true, value?.ToString() ?? "", 200);
    }
}
