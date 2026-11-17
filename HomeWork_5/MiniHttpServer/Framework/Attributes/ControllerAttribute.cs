namespace oris_hw1.framework.core;
[AttributeUsage(AttributeTargets.Class)]
public sealed class HttpController : Attribute
{
    public string? Name { get; }
    public HttpController(string? name = null) { Name = name; }
}
