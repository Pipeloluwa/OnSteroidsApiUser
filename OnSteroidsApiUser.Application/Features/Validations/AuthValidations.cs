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
