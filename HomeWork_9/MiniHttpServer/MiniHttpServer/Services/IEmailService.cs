namespace oris_hw1.framework.core;
public interface IEmailService
{
    Task SendAsync(string recipient);
}
