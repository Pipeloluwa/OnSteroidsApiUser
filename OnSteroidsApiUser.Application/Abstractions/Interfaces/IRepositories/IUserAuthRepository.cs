using OnSteroidsApiUser.Domain.Models.Auth;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IUserAuthRepository
{
    Task<UserAuthDto?> RegisterAsync(string email);
    Task<UserAuthDto?> SendOtpAsync(string email, string otp, DateTime expiresAt);
    Task<UserAuthDto?> VerifyOtpAsync(string email, string otp);
    Task<UserAuthDto?> GetByEmailAsync(string email);
    Task<UserAuthDto?> GetByIdAsync(Guid id);
    Task LogoutAsync(Guid id);
}
