using System.Text.Json;
namespace oris_hw1.framework.core;
public static class ConfigurationManager
{
    public static ServerSettings Load(string? path = null)
    {
        path ??= Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(path)) return new ServerSettings();
        return JsonSerializer.Deserialize<ServerSettings>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ServerSettings();
    }
}
