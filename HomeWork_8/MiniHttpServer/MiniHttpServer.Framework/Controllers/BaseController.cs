namespace oris_hw1.framework.core;
public abstract class BaseController
{
    protected HtmlResult Html(string text) => new HtmlResult(text);
    protected JsonResult Json(object? value) => new JsonResult(value);
}
