using OnSteroidsApiUser.Domain.Models.Auth;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IUserAuthRepository
{
    Task<UserAuthDto?> RegisterAsync(string email);
    Task<UserAuthDto?> SendOtpAsync(string email, string otp, DateTime expiresAt);
    Task<UserAuthDto?> VerifyOtpAsync(string email, string otp);
    Task<UserAuthDto?> SetPasswordAsync(string email, string passwordHash);
    Task<UserAuthDto?> ChangePasswordAsync(Guid id, string passwordHash);
    Task<UserAuthDto?> SetAuthenticatedAsync(Guid id);
    Task<UserAuthDto?> GetByEmailAsync(string email);
    Task<UserAuthDto?> GetByIdAsync(Guid id);
    Task LogoutAsync(Guid id);
    Task UpdateRefreshTokenAsync(Guid id, string refreshToken, DateTime expiry);
    Task<UserAuthDto?> GetByRefreshTokenAsync(string refreshToken);
}
