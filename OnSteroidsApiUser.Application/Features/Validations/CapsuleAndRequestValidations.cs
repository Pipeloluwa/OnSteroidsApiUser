using FluentValidation;
using OnSteroidsApiUser.Domain.Models.Capsule;
using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.Application.Features.Validations;

public class CreateCapsuleRequestValidator : AbstractValidator<CreateCapsuleRequest>
{
    public CreateCapsuleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Capsule name is required.")
            .MaximumLength(255).WithMessage("Capsule name cannot exceed 255 characters.");
    }
}

public class UpdateCapsuleRequestValidator : AbstractValidator<UpdateCapsuleRequest>
{
    public UpdateCapsuleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Capsule name is required.")
            .MaximumLength(255).WithMessage("Capsule name cannot exceed 255 characters.");
    }
}

public class SaveRequestStateRequestValidator : AbstractValidator<SaveRequestStateRequest>
{
    public SaveRequestStateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Request name is required.")
            .MaximumLength(255).WithMessage("Request name cannot exceed 255 characters.");
    }
}
