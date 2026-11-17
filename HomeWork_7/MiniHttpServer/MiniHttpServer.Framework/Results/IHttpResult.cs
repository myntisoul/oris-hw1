using System.Net;
namespace oris_hw1.framework.core;
public interface IHttpResult
{
    Task ExecuteAsync(HttpListenerResponse response);
}
