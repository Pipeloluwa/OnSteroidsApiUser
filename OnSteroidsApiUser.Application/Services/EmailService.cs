using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;

namespace OnSteroidsApiUser.Application.Services;

public class EmailService(
    IOptions<AppSettings> appSettings,
    ILogger<EmailService> logger
) : IEmailService
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<bool> SendEmailAsync(string emailReceiver, string emailSubject, string emailBody)
    {
        _logger.LogInformation("Attempting to send email to {EmailReceiver} with subject '{Subject}'", emailReceiver, emailSubject);

        _logger.LogInformation("[EMAIL DISPATCH TO {Receiver}]:\nSubject: {Subject}\nBody:\n{Body}",
            emailReceiver, emailSubject, emailBody);

        var emailSettings = _appSettings.EmailSettings;
        if (emailSettings == null || string.IsNullOrWhiteSpace(emailSettings.Host) || string.IsNullOrWhiteSpace(emailSettings.EmailSender))
        {
            _logger.LogWarning("Email settings are not fully configured. Email was logged to console.");
            return false;
        }

        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailSettings.EmailSender);
            email.From.Add(new MailboxAddress(emailSettings.DisplayName ?? "OnSteroids", emailSettings.EmailSender));
            email.To.Add(MailboxAddress.Parse(emailReceiver));
            email.Subject = emailSubject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailBody };

            using var smtp = new SmtpClient();
            if (!string.IsNullOrWhiteSpace(emailSettings.LocalDomain))
            {
                smtp.LocalDomain = emailSettings.LocalDomain;
            }

            var port = emailSettings.Port > 0 ? emailSettings.Port : 465;
            var secureOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(emailSettings.Host, port, secureOption);

            if (!string.IsNullOrEmpty(emailSettings.EmailSenderPassword))
            {
                await smtp.AuthenticateAsync(emailSettings.EmailSender, emailSettings.EmailSenderPassword);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {EmailReceiver}", emailReceiver);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {EmailReceiver}.", emailReceiver);
            return false;
        }
    }
}
