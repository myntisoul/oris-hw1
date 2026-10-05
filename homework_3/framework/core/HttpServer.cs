using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

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

                if (request.HttpMethod == "POST" && path == "/login")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string body = await reader.ReadToEndAsync();
                        Console.WriteLine(body);
                        string email = body.Split('=')[1].Split('&')[0];
                        string password = body.Split("=")[2];
                        email = email.Replace("%40", "@");
                        Console.WriteLine(email + ":" + password);

                        string responseText = "<html><body><h2>Успешно! Данные отправлены.</h2><a href='/login'>Назад</a></body></html>";
                        byte[] bufferFile = Encoding.UTF8.GetBytes(responseText);
                        response.ContentLength64 = bufferFile.Length;
                        response.ContentType = "text/html; charset=utf-8";
                        using Stream outputFile = response.OutputStream;
                        await outputFile.WriteAsync(bufferFile);
                        await outputFile.FlushAsync();
                    }

                }
                

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
}




















































































































