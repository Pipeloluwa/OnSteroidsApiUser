namespace OnSteroidsApiUser.Domain.Models.Auth;

public class SendOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
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
    public string? Otp { get; set; }
    public bool IsAuthenticated { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
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
}
