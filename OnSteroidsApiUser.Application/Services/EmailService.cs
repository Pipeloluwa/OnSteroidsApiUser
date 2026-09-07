using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;

namespace OnSteroidsApiUser.Application.Services;

public class EmailService(
    IOptions<AppSettings> appSettings,
     IAuthDetailsHelper authDetailsHelper,
    ILogger<EmailService> logger
) : IEmailService
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<bool> SendEmailAsync(string emailReceiver, string emailSubject, string emailBody)
    {
        _logger.LogInformation("Attempting to send email to {EmailReceiver} with subject '{Subject}'", emailReceiver, emailSubject);

        var emailSettings = _appSettings.EmailSettings;
        if (emailSettings == null || string.IsNullOrWhiteSpace(emailSettings.Host) || string.IsNullOrWhiteSpace(emailSettings.EmailSender))
        {
            _logger.LogWarning("[{RequestId}] Email settings are not fully configured. Email was logged to console.", _authDetails.RequestId);
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

            _logger.LogInformation("[{RequestId}] Email sent successfully to {EmailReceiver}", _authDetails.RequestId, emailReceiver);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Failed to send email via SMTP to {EmailReceiver}.", _authDetails.RequestId, emailReceiver);
            return false;
        }
    }
}
