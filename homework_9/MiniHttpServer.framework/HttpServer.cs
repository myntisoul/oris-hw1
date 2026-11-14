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
    private string filePath = Path.Combine("static", "index.html");
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
                string path = request.Url.LocalPath;
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

                string staticFilePath = Path.Combine(Directory.GetCurrentDirectory(), "static", path.TrimStart('/'));
                FileInfo fileInfo = new FileInfo(staticFilePath);

                if (!fileInfo.Exists)
                {
                    response.StatusCode = 404;
                    staticFilePath = Path.Combine(Directory.GetCurrentDirectory(), "static", "404.html");
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

    public async Task<(bool isHandled, string responseHtml, int code)> TryRoute(string httpMethod, string path, string body, NameValueCollection queryString)
    {
        if (path.Contains(".") || path == "/") return (false, null, 200);

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return (false, null, 200);

        string controllerName = segments[0];
        string actionName = segments.Length > 1 ? segments[1] : "index";

        var assembly = Assembly.GetExecutingAssembly();
        var controller = assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
            .FirstOrDefault(c =>
            {
                var name = c.GetCustomAttribute<HttpController>().Name;
                return (name ?? c.Name.Replace("Controller", "")).ToLower() == controllerName.ToLower();
            });

        if (controller == null) return (false, null, 200);

        var method = controller.GetMethods()
            .Where(t => t.Name.ToLower() == actionName.ToLower())
            .FirstOrDefault(t => t.GetCustomAttributes(true)
                .Any(attr => string.Equals(attr.GetType().Name, $"Http{httpMethod}", StringComparison.OrdinalIgnoreCase)));

        if (method == null) return (false, null, 200);

        NameValueCollection form = new NameValueCollection();
        if (httpMethod == "POST" || httpMethod == "PUT")
        {
            if (!string.IsNullOrEmpty(body))
            {
                foreach (var pair in body.Split('&'))
                {
                    var kv = pair.Split(new[] { '=' }, 2);
                    if (kv.Length == 2)
                        form.Add(kv[0], Uri.UnescapeDataString(kv[1].Replace("+", " ")));
                }
            }
        }

        object[] queryParams = method.GetParameters().Select(p =>
        {
            if (p.ParameterType == typeof(NameValueCollection))
            {
                return (object)(httpMethod == "GET" ? queryString : form);
            }
            if (p.ParameterType == typeof(string) && p.Name == "body") return body;
            return null;
        }).ToArray();

        var constructor = controller.GetConstructors().First();

        var ctorParams = constructor.GetParameters();
        object[] ctorArgs = new object[ctorParams.Length];

        //for (int i = 0; i < ctorParams.Length; i++)
        //{
        //    Type paramType = ctorParams[i].ParameterType;

        //    if (paramType == typeof(IEmailSender))
        //    {
        //        ctorArgs[i] = EmailSender;
        //    }
        //    else
        //    {
        //        ctorArgs[i] = null;
        //    }
        //}


        var controllerInstance = Activator.CreateInstance(controller, ctorArgs);
        var ret = method.Invoke(controllerInstance, queryParams);

        if (ret is Task task)
        {
            await task;
            var resultProp = task.GetType().GetProperty("Result");
            ret = resultProp?.GetValue(task);
        }

        if (ret is ValueTuple<string, int> resultTuple)
            return (true, resultTuple.Item1, resultTuple.Item2);

        string responseHtml = ret?.ToString();
        return (true, responseHtml, 200);
    }
}





















































































































