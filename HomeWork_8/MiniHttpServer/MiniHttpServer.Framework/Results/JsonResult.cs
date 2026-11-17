using System.Net;
using System.Text.Json;
namespace oris_hw1.framework.core;
public sealed class JsonResult : IHttpResult
{
    private readonly object? _value;
    public JsonResult(object? value) { _value = value; }
    public async Task ExecuteAsync(HttpListenerResponse response)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(_value);
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = data.Length;
        await response.OutputStream.WriteAsync(data);
    }
}
