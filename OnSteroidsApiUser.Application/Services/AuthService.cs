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
    IValidator<SetPasswordRequest> setPasswordValidator,
    IValidator<PasswordLoginRequest> passwordLoginValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator,
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
    private readonly IValidator<SetPasswordRequest> _setPasswordValidator = setPasswordValidator;
    private readonly IValidator<PasswordLoginRequest> _passwordLoginValidator = passwordLoginValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator = forgotPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator = changePasswordValidator;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<AuthService> _logger = logger;

    private const int Pbkdf2Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    private int GetOtpCooldownSeconds() =>
        Math.Max(1, (_appSettings.OtpSettings?.RateLimit?.Generate?.WindowMinutes ?? 1) * 60);

    public async Task<(int StatusCode, object Response)> CheckEmailAsync(SendOtpRequest request)
    {
        var validation = await _sendOtpValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userAuthRepo.RegisterAsync(email);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Failed to check user account", null);
        }

        return BaseResponseHelpers.ReturnSuccess<object>(
            user.HasPassword ? "Enter your password to sign in." : "Proceed with email verification.",
            new
            {
                email,
                hasPassword = user.HasPassword,
                otpSent = false
            });
    }

    public async Task<(int StatusCode, object Response)> SendOtpAsync(SendOtpRequest request)
    {
        var validation = await _sendOtpValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        // Ensure user is registered or exists
        var user = await _userAuthRepo.RegisterAsync(email);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Failed to register user", null);
        }

        // Strict password sign-in: if user already has a password, prompt for password sign-in.
        // Password resets use ForgotPasswordAsync instead of SendOtpAsync.
        if (user.HasPassword)
        {
            _logger.LogInformation("[{RequestId}] Returning user {Email} has password; prompting password sign-in", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnSuccess<object>("Enter your password to sign in.", new
            {
                email,
                hasPassword = true,
                otpSent = false
            });
        }

        // Generate secure 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var expiryMinutes = _appSettings.OtpSettings?.OtpExpiryMinutes ?? 10;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Save OTP in DB
        var updated = await _userAuthRepo.SendOtpAsync(email, otp, expiresAt);
        if (updated == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Sorry something went wrong, could not complete operation, please try again.", null);
        }

        // Send email
        var subject = _appSettings.OtpSettings?.EmailSubject ?? "Your OnSteroids Login Code";
        var bodyTemplate = _appSettings.OtpSettings?.EmailBody ?? "Your login code is {otpToken}. It expires in {otpMinute} minutes.";
        var body = bodyTemplate
            .Replace("{otpToken}", otp)
            .Replace("{otpMinute}", expiryMinutes.ToString());

        var retryAfterSeconds = GetOtpCooldownSeconds();

        _logger.LogInformation("[{RequestId}] OTP dispatched successfully for {Email}", _authDetails.RequestId, email);
        return (await _emailService.SendEmailAsync(email, subject, body))
            ? BaseResponseHelpers.ReturnSuccess<object>($"Verification code sent to {email}.", new
            {
                email,
                hasPassword = user.HasPassword,
                otpSent = true,
                retryAfterSeconds
            })
            : BaseResponseHelpers.ReturnServerErrorData("Sorry something went wrong, could not complete operation, please try again.", null);
    }

    public async Task<(int StatusCode, object Response)> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userAuthRepo.RegisterAsync(email);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Could not process password reset request.", null);
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var expiryMinutes = _appSettings.OtpSettings?.OtpExpiryMinutes ?? 10;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var updated = await _userAuthRepo.SendOtpAsync(email, otp, expiresAt);
        if (updated == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Could not send password reset code. Please try again.", null);
        }

        var subject = "Reset Your OnSteroids Password";
        var bodyTemplate = _appSettings.OtpSettings?.EmailBody ?? "Your password reset code is {otpToken}. It expires in {otpMinute} minutes.";
        var body = bodyTemplate
            .Replace("{otpToken}", otp)
            .Replace("{otpMinute}", expiryMinutes.ToString());

        var retryAfterSeconds = GetOtpCooldownSeconds();

        _logger.LogInformation("[{RequestId}] Password reset OTP dispatched for {Email}", _authDetails.RequestId, email);
        return (await _emailService.SendEmailAsync(email, subject, body))
            ? BaseResponseHelpers.ReturnSuccess<object>($"Password reset code sent to {email}.", new
            {
                email,
                hasPassword = user.HasPassword,
                otpSent = true,
                isPasswordReset = true,
                retryAfterSeconds
            })
            : BaseResponseHelpers.ReturnServerErrorData("Could not send password reset code. Please try again.", null);
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

            // OTP verification strictly unlocks password creation (for new users) or password reset.
            // Sign-in is strictly performed via password.
            var pendingSetupResponse = new AuthLoginResponse
            {
                User = user,
                Token = string.Empty,
                RefreshToken = string.Empty,
                ExpiresAt = DateTime.UtcNow,
                RequiresPasswordSetup = true
            };

            var promptMsg = user.HasPassword
                ? "Code verified! Enter your new password below."
                : "Email verified! Create a password to finish setting up your account.";

            _logger.LogInformation("[{RequestId}] OTP verified for {Email}; awaiting password setup", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnSuccess(promptMsg, pendingSetupResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{RequestId}] Failed to verify OTP for {Email}", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(ex.Message, null);
        }
    }

    public async Task<(int StatusCode, object Response)> SetPasswordAsync(SetPasswordRequest request)
    {
        var validation = await _setPasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BaseResponseHelpers.ReturnValidationSyntaxError(errors.FirstOrDefault() ?? "Password does not meet security requirements.", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        try
        {
            var existingUser = await _userAuthRepo.GetByEmailAsync(email);
            if (existingUser == null)
            {
                return BaseResponseHelpers.ReturnNotFoundError("Account not found.", null);
            }

            if (!existingUser.IsOtpVerified)
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData("Please verify your email with the 6-digit OTP code before setting a password.", null);
            }

            if (!string.IsNullOrWhiteSpace(existingUser.PasswordHash) && VerifyPasswordHash(request.Password, existingUser.PasswordHash))
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData("New password cannot be the same as your previous password.", null);
            }

            var passwordHash = HashPassword(request.Password);
            var updatedUser = await _userAuthRepo.SetPasswordAsync(email, passwordHash);
            if (updatedUser == null)
            {
                return BaseResponseHelpers.ReturnServerErrorData("Could not save password. Please try again.", null);
            }

            var (token, expiresAt) = _jwtTokenHelper.GenerateToken(updatedUser.Id, updatedUser.Email);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userAuthRepo.UpdateRefreshTokenAsync(updatedUser.Id, refreshToken, refreshTokenExpiry);

            var authResponse = new AuthLoginResponse
            {
                User = updatedUser,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                RequiresPasswordSetup = false
            };

            _logger.LogInformation("[{RequestId}] Account password set and user {Email} authenticated", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnSuccess("Password saved and signed in successfully.", authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{RequestId}] Failed to set password for {Email}", _authDetails.RequestId, email);
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(ex.Message, null);
        }
    }

    public async Task<(int StatusCode, object Response)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BaseResponseHelpers.ReturnValidationSyntaxError(errors.FirstOrDefault() ?? "Password does not meet security requirements.", errors);
        }

        var user = await _userAuthRepo.GetByIdAsync(userId);
        if (user == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("User not found.", null);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !VerifyPasswordHash(request.CurrentPassword, user.PasswordHash))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("Current password is incorrect.", null);
        }

        if (request.NewPassword.Equals(user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("New password cannot be the same as your email address.", null);
        }

        if (VerifyPasswordHash(request.NewPassword, user.PasswordHash))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("New password must be different from your current password.", null);
        }

        var newPasswordHash = HashPassword(request.NewPassword);
        var updatedUser = await _userAuthRepo.ChangePasswordAsync(userId, newPasswordHash);
        if (updatedUser == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Could not update password. Please try again.", null);
        }

        _logger.LogInformation("[{RequestId}] User {Email} changed password successfully", _authDetails.RequestId, user.Email);
        return BaseResponseHelpers.ReturnSuccess("Password changed successfully.", updatedUser);
    }

    public async Task<(int StatusCode, object Response)> LoginWithPasswordAsync(PasswordLoginRequest request)
    {
        var validation = await _passwordLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BaseResponseHelpers.ReturnValidationSyntaxError(errors.FirstOrDefault() ?? "Invalid login credentials.", errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userAuthRepo.GetByEmailAsync(email);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("Invalid email or password.", null);
        }

        if (!VerifyPasswordHash(request.Password, user.PasswordHash))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("Invalid email or password.", null);
        }

        var authenticatedUser = await _userAuthRepo.SetAuthenticatedAsync(user.Id) ?? user;
        authenticatedUser.IsAuthenticated = true;
        authenticatedUser.HasPassword = true;

        var (token, expiresAt) = _jwtTokenHelper.GenerateToken(authenticatedUser.Id, authenticatedUser.Email);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userAuthRepo.UpdateRefreshTokenAsync(authenticatedUser.Id, refreshToken, refreshTokenExpiry);

        var authResponse = new AuthLoginResponse
        {
            User = authenticatedUser,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            RequiresPasswordSetup = false
        };

        _logger.LogInformation("[{RequestId}] User {Email} signed in with password", _authDetails.RequestId, email);
        return BaseResponseHelpers.ReturnSuccess("Signed in successfully.", authResponse);
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"PBKDF2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPasswordHash(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2")
                return false;

            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
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

    public async Task<(int StatusCode, object Response)> RefreshAsync(RefreshRequest request)
    {
        var user = await _userAuthRepo.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("Invalid or expired refresh token", null);
        }

        var (token, expiresAt) = _jwtTokenHelper.GenerateToken(user.Id, user.Email);
        
        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userAuthRepo.UpdateRefreshTokenAsync(user.Id, newRefreshToken, newRefreshTokenExpiry);

        var authResponse = new AuthLoginResponse
        {
            User = user,
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt
        };

        return BaseResponseHelpers.ReturnSuccess("Token refreshed successfully", authResponse);
    }
}
