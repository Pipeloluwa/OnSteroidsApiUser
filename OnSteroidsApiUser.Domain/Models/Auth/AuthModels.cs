using System.Text.Json.Serialization;

namespace OnSteroidsApiUser.Domain.Models.Auth;

public class SendOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public bool ForceOtp { get; set; } = false;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public bool IsPasswordReset { get; set; } = false;
}

public class SetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class PasswordLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserAuthDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    public string? Otp { get; set; }
    public bool IsAuthenticated { get; set; }
    [JsonIgnore]
    public DateTime? OtpExpiresAt { get; set; }
    [JsonIgnore]
    public string? PasswordHash { get; set; }
    public bool IsOtpVerified { get; set; }
    public bool HasPassword { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    [JsonIgnore]
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuthLoginResponse
{
    public UserAuthDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool RequiresPasswordSetup { get; set; }
}

