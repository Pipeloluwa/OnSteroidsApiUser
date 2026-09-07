namespace OnSteroidsApiUser.Domain.Models.AppSettingsModels;

public class AppSettings
{
    public string? AllowedHosts { get; set; }
    public string? AllowedOrigins { get; set; }
    public SqlSettings? SqlSettings { get; set; }
    public JWTSettings? JWTSettings { get; set; }
    public OtpSettings? OtpSettings { get; set; }
    public EmailSettings? EmailSettings { get; set; }
}

public class SqlSettings
{
    public string? ConnectionString { get; set; }
}

public class JWTSettings
{
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Subject { get; set; }
    public int TokenExpiryMinutes { get; set; } = 1440; // 24 hours default
    public string? TokenType { get; set; } = "Bearer";
    public int RefreshTokenExpiryMinutes { get; set; } = 43200; // 30 days
}

public class OtpSettings
{
    public int OtpExpiryMinutes { get; set; } = 10;
    public string? EmailSubject { get; set; } = "Your OnSteroids Login Code";
    public string? EmailBody { get; set; } = "Your verification code is: {otpToken}. It will expire in {otpMinute} minutes.";
}

public class EmailSettings
{
    public string? DisplayName { get; set; }
    public string? LocalDomain { get; set; }
    public string? EmailSender { get; set; }
    public string? EmailSenderPassword { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 465;
}
