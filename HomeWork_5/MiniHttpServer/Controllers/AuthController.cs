using oris_hw1.framework.core;
namespace oris_hw1.framework.core;
[HttpController("auth")]
public sealed class AuthController
{
    [HttpGET]
    public string Index() => "<h1>Авторизация</h1>";
    [HttpPOST]
    public string Login(string email, string password)
    {
        return string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)
            ? "<h1>Введите email и пароль</h1>" : "<h1>Авторизация выполнена</h1>";
    }
}
