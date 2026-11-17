using oris_hw1.framework.core;
namespace oris_hw1.framework.core;
[HttpController("users")]
public sealed class UsersController
{
    [HttpGET]
    public string Index()
    {
        string file = Path.Combine(AppContext.BaseDirectory, "Templates", "User", "List.html");
        return File.Exists(file) ? File.ReadAllText(file) : "<h1>Users</h1>";
    }
}
