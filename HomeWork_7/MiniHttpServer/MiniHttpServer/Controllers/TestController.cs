using oris_hw1.framework.core;
namespace oris_hw1.framework.core;
[HttpController("test")]
public sealed class TestController
{
    [HttpGET]
    public string Index() => "<h1>Test</h1>";
}
