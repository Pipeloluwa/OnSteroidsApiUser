namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string emailReceiver, string emailSubject, string emailBody);
}
