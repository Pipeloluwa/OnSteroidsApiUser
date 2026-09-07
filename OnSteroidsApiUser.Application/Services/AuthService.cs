using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using OnSteroidsApiUser.Domain.Models.Auth;
using System.Security.Cryptography;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class AuthService(
    IUserAuthRepository userAuthRepository,
    IJwtTokenHelper jwtTokenHelper,
    IEmailService emailService,
    IOptions<AppSettings> appSettings,
    IValidator<SendOtpRequest> sendOtpValidator,
    IValidator<VerifyOtpRequest> verifyOtpValidator,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly IUserAuthRepository _userAuthRepo = userAuthRepository;
    private readonly IJwtTokenHelper _jwtTokenHelper = jwtTokenHelper;
    private readonly IEmailService _emailService = emailService;
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly IValidator<SendOtpRequest> _sendOtpValidator = sendOtpValidator;
    private readonly IValidator<VerifyOtpRequest> _verifyOtpValidator = verifyOtpValidator;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<(int StatusCode, object Response)> SendOtpAsync(SendOtpRequest request)
    {
        var validation = await _sendOtpValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Ensure user is registered or exists
        var user = await _userAuthRepo.RegisterAsync(email);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Failed to register user", null);
        }

        // 2. Generate secure 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var expiryMinutes = _appSettings.OtpSettings?.OtpExpiryMinutes ?? 10;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // 3. Save OTP in DB
        var updated = await _userAuthRepo.SendOtpAsync(email, otp, expiresAt);
        if (updated == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Sorry something went wrong, could not complete operation, please try again.", null);
        }

        // 4. Send email
        var subject = _appSettings.OtpSettings?.EmailSubject ?? "Your OnSteroids Login Code";
        var bodyTemplate = _appSettings.OtpSettings?.EmailBody ?? "Your login code is {otpToken}. It expires in {otpMinute} minutes.";
        var body = bodyTemplate
            .Replace("{otpToken}", otp)
            .Replace("{otpMinute}", expiryMinutes.ToString());

        _logger.LogInformation("[{RequestId}] OTP dispatched successfully for {Email}", _authDetails.RequestId, email);
        return (await _emailService.SendEmailAsync(email, subject, body)) ? BaseResponseHelpers.ReturnSuccess<object>($"OTP sent to {email}. Valid for {expiryMinutes} minutes.", new
        {
            email,
            expiresAt
        }) :   BaseResponseHelpers.ReturnServerErrorData("Sorry something went wrong, could not complete operation, please try again.", null); ;
    }

    public async Task<(int StatusCode, object Response)> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var validation = await _verifyOtpValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var otp = request.Otp.Trim();

        try
        {
            var user = await _userAuthRepo.VerifyOtpAsync(email, otp);
            if (user == null)
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData("Invalid or expired OTP code.", null);
            }

            var (token, expiresAt) = _jwtTokenHelper.GenerateToken(user.Id, user.Email);

            var authResponse = new AuthLoginResponse
            {
                User = user,
                Token = token,
                ExpiresAt = expiresAt
            };

            var safeUser = new { user.Id, user.Email, user.IsAuthenticated, user.CreatedAt };
            _logger.LogInformation("[{RequestId}] User {Email} successfully authenticated: {User}", _authDetails.RequestId, email, JsonSerializer.Serialize(safeUser));
            return BaseResponseHelpers.ReturnSuccess("Authentication successful", authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{RequestId}] Failed to verify OTP for {Email}", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(ex.Message, null);
        }
    }

    public async Task<(int StatusCode, object Response)> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userAuthRepo.GetByIdAsync(userId);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("User not found", null);
        }

        return BaseResponseHelpers.ReturnSuccess("User retrieved", user);
    }

    public async Task<(int StatusCode, object Response)> LogoutAsync(Guid userId)
    {
        await _userAuthRepo.LogoutAsync(userId);
        _logger.LogInformation("[{RequestId}] User {UserId} logged out successfully", _authDetails.RequestId, userId);
        return BaseResponseHelpers.ReturnSuccess<object>("Logged out successfully", null);
    }
}
