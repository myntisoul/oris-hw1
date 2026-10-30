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
    private CancellationTokenSource cts = new CancellationTokenSource();

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
                

                (bool isHandledByController, string htmlResponse) = await TryRoute(httpMethod, path, body);
                if (isHandledByController) { Receive(); return; }
                    
                //if (request.HttpMethod == "POST" && path == "/login")
                //{
                //    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                //    {
                //        string body = await reader.ReadToEndAsync();
                //        Console.WriteLine(body);
                //string email = body.Split('=')[1].Split('&')[0];
                //string password = body.Split("=")[2];
                //email = email.Replace("%40", "@");
                //Console.WriteLine(email + ":" + password);

                //        string responseText = "<html><body><h2>Успешно! Данные отправлены.</h2><a href='/login'>Назад</a></body></html>";
                //        byte[] bufferFile = Encoding.UTF8.GetBytes(responseText);
                //        response.ContentLength64 = bufferFile.Length;
                //        response.ContentType = "text/html; charset=utf-8";
                //        using Stream outputFile = response.OutputStream;
                //        await outputFile.WriteAsync(bufferFile);
                //        await outputFile.FlushAsync();
                //    }

                //}


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

    private async Task<(bool isHandled, string responseHtml)> TryRoute(string httpMethod, string path, string body)
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
                //Console.WriteLine($"[DEBUG] параметр: {parameters[i].Name}, тип: {parameters[i].ParameterType}");
                //Console.WriteLine($"[DEBUG] QueryString[{parameters[i].Name}] = {context.Request.QueryString[parameters[i].Name]}");
                string queryValue = 
                Console.WriteLine(queryValue);
                string value = null;

                if (queryValue != null) value = queryValue;
                else if (i < strParams.Length) value = strParams[i];

                if (value != null)
                    queryParams[i] = Convert.ChangeType(value, parameters[i].ParameterType);
                else
                    queryParams[i] = parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
            }
        }
        var controllerInstance = Activator.CreateInstance(controller);
        var ret = method.Invoke(controllerInstance, queryParams);

        if (ret is Task task)
        {
            await task;
            // Если Task<T>, достаём Result
            var resultProp = task.GetType().GetProperty("Result");
            if (resultProp != null)
                ret = resultProp.GetValue(task);
            else
                ret = null; // Task без результата
        }

        if (ret != null)
        {
            string responseText = ret.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseText);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = "text/html; charset=utf-8";
            using Stream output = context.Response.OutputStream;
            output.Write(buffer);
            output.Flush();
        }
        return true;
    }
}




















































































































