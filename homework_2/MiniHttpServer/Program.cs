using oris_hw1;
using oris_hw1.framework.core;
namespace oris;
internal static class Program
{
    private static void Main()
    {
        var server = new HttpServer();
        server.Start();
        Console.WriteLine("Для остановки введите stop");
        while (Console.ReadLine() is { } command)
        {
            if (!string.Equals(command, "stop", StringComparison.OrdinalIgnoreCase)) continue;
            server.Stop();
            return;
        }
        server.Stop();
    }
}
