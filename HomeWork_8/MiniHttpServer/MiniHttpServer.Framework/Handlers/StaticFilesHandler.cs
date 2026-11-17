using System.Net;
namespace oris_hw1.framework.core;
public sealed class StaticFilesHandler : RequestHandler
{
    public override async Task<bool> HandleAsync(HttpListenerContext context)
    {
        var relative = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/").TrimStart('/');
        if (relative.Length == 0) relative = "index.html";
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "static"));
        var path = Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.Ordinal) && path != basePath)
            return await base.HandleAsync(context);
        if (!File.Exists(path)) return await base.HandleAsync(context);
        context.Response.ContentType = MimeTypeProvider.GetMimeType(path);
        context.Response.ContentLength64 = new FileInfo(path).Length;
        await using var stream = File.OpenRead(path);
        await stream.CopyToAsync(context.Response.OutputStream);
        return true;
    }
}
