using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.Auth;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class UserAuthRepository(
    IDapperConnection dapperConnection,
    ILogger<UserAuthRepository> logger
) : IUserAuthRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<UserAuthRepository> _logger = logger;

    public async Task<UserAuthDto?> RegisterAsync(string email)
    {
        _logger.LogInformation("Registering or fetching user {Email}", email);
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        return await _dapper.Query<UserAuthDto>(parameters, "dbo.spUserAuth_Register");
    }

    public async Task<UserAuthDto?> SendOtpAsync(string email, string otp, DateTime expiresAt)
    {
        _logger.LogInformation("Saving OTP for {Email}", email);
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        parameters.Add("@Otp", otp);
        parameters.Add("@OtpExpiresAt", expiresAt);
        return await _dapper.Query<UserAuthDto>(parameters, "dbo.spUserAuth_SendOtp");
    }

    public async Task<UserAuthDto?> VerifyOtpAsync(string email, string otp)
    {
        _logger.LogInformation("Verifying OTP for {Email}", email);
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        parameters.Add("@Otp", otp);
        return await _dapper.Query<UserAuthDto>(parameters, "dbo.spUserAuth_VerifyOtp");
    }

    public async Task<UserAuthDto?> GetByEmailAsync(string email)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        return await _dapper.Query<UserAuthDto>(parameters, "dbo.spUserAuth_GetByEmail");
    }

    public async Task<UserAuthDto?> GetByIdAsync(Guid id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _dapper.Query<UserAuthDto>(parameters, "dbo.spUserAuth_GetById");
    }

    public async Task LogoutAsync(Guid id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        await _dapper.Execute(parameters, "dbo.spUserAuth_Logout");
    }
}
