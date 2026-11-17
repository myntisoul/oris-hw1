namespace oris_hw1.framework.core;
public sealed class RouteTable
{
    private readonly Dictionary<string, Type> _controllers = new(StringComparer.OrdinalIgnoreCase);
    public void Register(string name, Type controllerType) => _controllers[name] = controllerType;
    public bool TryFind(string name, out Type? controllerType) => _controllers.TryGetValue(name, out controllerType);
}
