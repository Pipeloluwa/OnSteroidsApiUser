namespace OnSteroidsApiUser.Domain.Models.AppSettingsModels;

public class AppSettings
{
    public string? AllowedHosts { get; set; }
    public string? AllowedOrigins { get; set; }
    public SqlSettings? SqlSettings { get; set; }
    public JWTSettings? JWTSettings { get; set; }
    public OtpSettings? OtpSettings { get; set; }
    public EmailSettings? EmailSettings { get; set; }
    public RateLimitPolicySettings? GlobalRateLimiter { get; set; } = new();
    public LogSettings? LogSettings { get; set; } = new();
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
    public OtpRateLimitSettings? RateLimit { get; set; } = new();
}

public class OtpRateLimitSettings
{
    public RateLimitPolicySettings Generate { get; set; } = new()
    {
        PermitLimit = 1,
        WindowMinutes = 1,
        ErrorMessage = "Too many OTP requests. Please wait {retryAfterSeconds}s before requesting a new code."
    };

    public RateLimitPolicySettings Validate { get; set; } = new()
    {
        PermitLimit = 3,
        WindowMinutes = 15,
        ErrorMessage = "Too many OTP verification attempts. Please try again after {retryAfterMinutes} minute(s)."
    };
}

public class RateLimitPolicySettings
{
    /// <summary>Maximum number of requests allowed per client within the window.</summary>
    public int PermitLimit { get; set; }

    /// <summary>Length of the fixed window in minutes.</summary>
    public int WindowMinutes { get; set; }

    /// <summary>Maximum number of queued requests allowed.</summary>
    public int QueueLimit { get; set; } = 2;

    /// <summary>Message returned when the limit is exceeded. Supports the {retryAfterMinutes} placeholder.</summary>
    public string? ErrorMessage { get; set; }
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

public class LogSettings
{
    public string? MinimumLevel { get; set; } = "Information";
    public string LogDirectory { get; set; } = "Logs";
    public ConsoleLogSettings Console { get; set; } = new();
    public StructuredLogSettings Structured { get; set; } = new();
    public UnstructuredLogSettings Unstructured { get; set; } = new();
    public DatabaseLogSettings Database { get; set; } = new();
}

public class ConsoleLogSettings
{
    public bool Enabled { get; set; } = true;
    public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}";
}

public class StructuredLogSettings
{
    public bool Enabled { get; set; } = true;
    public string Folder { get; set; } = "Structured";
    public string FileName { get; set; } = "log-.json";
    public string RollingInterval { get; set; } = "Day";
    public int? RetainedFileCountLimit { get; set; } = 31;
    public long? FileSizeLimitBytes { get; set; } = 104857600; // 100MB
    public bool RollOnFileSizeLimit { get; set; } = true;
}

public class UnstructuredLogSettings
{
    public bool Enabled { get; set; } = true;
    public string Folder { get; set; } = "Unstructured";
    public string FileName { get; set; } = "log-.txt";
    public string RollingInterval { get; set; } = "Day";
    public int? RetainedFileCountLimit { get; set; } = 31;
    public long? FileSizeLimitBytes { get; set; } = 104857600; // 100MB
    public bool RollOnFileSizeLimit { get; set; } = true;
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}";
}

public class DatabaseLogSettings
{
    public bool Enabled { get; set; } = true;
    public string StoredProcedure { get; set; } = "spApiLogs_Insert";
}

