namespace jumwebapi.Infrastructure.HttpClients.Mail;

public interface ISmtpEmailClient
{
    Task SendAsync(Email email);
}
