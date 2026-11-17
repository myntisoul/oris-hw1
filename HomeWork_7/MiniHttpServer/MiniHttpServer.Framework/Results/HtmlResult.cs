using System.Net;
using System.Text;
namespace oris_hw1.framework.core;
public sealed class HtmlResult : IHttpResult
{
    private readonly string _html;
    public HtmlResult(string html) { _html = html; }
    public async Task ExecuteAsync(HttpListenerResponse response)
    {
        var data = Encoding.UTF8.GetBytes(_html);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = data.Length;
        await response.OutputStream.WriteAsync(data);
    }
}
