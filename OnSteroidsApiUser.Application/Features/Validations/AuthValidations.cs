using FluentValidation;
using OnSteroidsApiUser.Domain.Models.Auth;

namespace OnSteroidsApiUser.Application.Features.Validations;

public class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .MinimumLength(4).WithMessage("OTP must be at least 4 characters.")
            .MaximumLength(10).WithMessage("OTP cannot exceed 10 characters.");
    }
}

public class SetPasswordRequestValidator : AbstractValidator<SetPasswordRequest>
{
    public SetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z).")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z).")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number (0-9).")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character (e.g., !@#$%^&*).")
            .Must((req, pwd) => string.IsNullOrEmpty(pwd) || !pwd.Equals(req.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password cannot be the same as your email address.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public class PasswordLoginRequestValidator : AbstractValidator<PasswordLoginRequest>
{
    public PasswordLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter (A-Z).")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter (a-z).")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one number (0-9).")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character (e.g., !@#$%^&*).")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from your current password.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your new password.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

