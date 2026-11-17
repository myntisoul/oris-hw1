namespace oris_hw1.framework.core;
public sealed class ServerSettings
{
    public string[] Prefixes { get; set; } = ["http://127.0.0.1:8888/"];
    public string StaticFilesPath { get; set; } = "static";
}
