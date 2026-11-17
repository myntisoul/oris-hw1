namespace oris_hw1.framework.core;
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpPOST : Attribute { }
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpPUT : Attribute { }
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpDELETE : Attribute { }
