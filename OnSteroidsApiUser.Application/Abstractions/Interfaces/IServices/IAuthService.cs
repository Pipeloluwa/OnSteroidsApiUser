using OnSteroidsApiUser.Domain.Models.Auth;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IAuthService
{
    Task<(int StatusCode, object Response)> SendOtpAsync(SendOtpRequest request);
    Task<(int StatusCode, object Response)> VerifyOtpAsync(VerifyOtpRequest request);
    Task<(int StatusCode, object Response)> GetCurrentUserAsync(Guid userId);
    Task<(int StatusCode, object Response)> LogoutAsync(Guid userId);
}
