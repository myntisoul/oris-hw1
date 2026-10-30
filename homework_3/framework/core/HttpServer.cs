using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Xml.Linq;
using TestProject1;

namespace oris_hw1.framework.core;

public class HttpServer
{
    private int _port = 8888;
    private string filePath = Path.Combine("static", "index.html"); 
    private HttpListener _listener;
    private CancellationTokenSource cts = new CancellationTokenSource();
    public IEmailSender EmailSender { get; set; } = new RealEmailSender();

    public void Start()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Ошибка: файл {filePath} не найден!");
            Console.WriteLine("Сервер не может быть запущен.");
            Console.ReadLine();
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
        try
        {
            if (_listener.IsListening)
            {
                var context = _listener.EndGetContext(result);
                var request = context.Request;
                string path = request.Url.LocalPath;
                var response = context.Response;
                Console.WriteLine("Пришел запрос");
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();
                string httpMethod = context.Request.HttpMethod;
                NameValueCollection queryString = request.QueryString;

                (bool isHandledByController, string htmlResponse) = await TryRoute(httpMethod, path, body, queryString);
                Console.WriteLine(isHandledByController);
                if (isHandledByController) { Receive(); return; }
                    
                if (string.IsNullOrEmpty(path) || path.EndsWith("/"))
                {
                    path += "index.html";
                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "static", path.TrimStart('/'));

                FileInfo fileInfo = new FileInfo(filePath);


                if (!fileInfo.Exists)
                {
                    response.StatusCode = 404;
                    filePath = Directory.GetCurrentDirectory() + $"/static/404.html";
                }

                switch (fileInfo.Extension)
                {
                    case ".html":
                        response.ContentType = "text/html; charset=utf-8";
                        break;
                    case ".css":
                        response.ContentType = "text/css; charset=utf-8";
                        break;
                    case ".js":
                        response.ContentType = "text/javascript; charset=utf-8";
                        break;
                    case ".png":
                        response.ContentType = "image/png";
                        break;
                    case ".ico":
                        response.ContentType = "image/x-icon";
                        break;
                    case ".svg":
                        response.ContentType = "image/svg+xml";
                        break;
                    case ".jpg":
                        response.ContentType = "image/jpeg";
                        break;
                }

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();

                Console.WriteLine("Запрос обработан");
                Receive();
            }
        }
        catch (HttpListenerException)
        {
            Console.WriteLine("stopped");
        }
    }

    public async Task<(bool isHandled, string responseHtml)> TryRoute(string httpMethod, string path, string body, NameValueCollection queryString)
    {
        if (path.Contains(".") || path == "/") return (false, null);

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return (false, null);
        string controllerName = segments[0];
        string[] strParams = segments.Skip(1).ToArray();

        var assembly = Assembly.GetExecutingAssembly();
        var controller = assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
            .FirstOrDefault(c => c.GetCustomAttribute<HttpController>().Name.ToLower() == controllerName.ToLower());
        if (controller == null) return (false, null);

        var method = controller.GetMethods()
            .Where(t => t.GetCustomAttributes(true)
                .Any(attr => string.Equals(attr.GetType().Name,
                              $"Http{httpMethod}",
                              StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault();
        if (method == null) return (false, null);
        object[] queryParams;

        if (httpMethod == "POST")
        {
            Console.WriteLine(body);
            string email = body.Split('=')[1].Split('&')[0];
            string password = body.Split("=")[2];
            email = email.Replace("%40", "@");
            Console.WriteLine(email + ":" + password);
            queryParams = new object[] { email, password };
        }
        else
        {
            var parameters = method.GetParameters();
            //foreach (var par in parameters) Console.Write(par.Name + " ");
            queryParams = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                string value = null;

                if (queryString != null) value = queryString[parameters[i].Name];
                else if (i < strParams.Length) value = strParams[i];
                Console.WriteLine(value);
                Console.WriteLine();
                if (value != null)
                    queryParams[i] = Convert.ChangeType(value, parameters[i].ParameterType);
                else
                    queryParams[i] = parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
            }
        }
        var constructor = controller.GetConstructors().First();

        var ctorParams = constructor.GetParameters();
        object[] ctorArgs = new object[ctorParams.Length];

        for (int i = 0; i < ctorParams.Length; i++)
        {
            Type paramType = ctorParams[i].ParameterType;

            if (paramType == typeof(IEmailSender))
            {
                ctorArgs[i] = EmailSender;
            }
            else
            {
                ctorArgs[i] = null;
            }
        }
        var controllerInstance = Activator.CreateInstance(controller, ctorArgs);
        var ret = method.Invoke(controllerInstance, queryParams);

        if (ret is Task task)
        {
            await task;
            var resultProp = task.GetType().GetProperty("Result");
            ret = resultProp?.GetValue(task);
        }

        string responseHtml = ret?.ToString();
        return (true, responseHtml);
    }
}




















































































































