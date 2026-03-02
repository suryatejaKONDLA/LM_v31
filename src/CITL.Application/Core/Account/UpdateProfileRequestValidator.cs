using FluentValidation;

namespace CITL.Application.Core.Account;

/// <summary>
/// FluentValidation validator for <see cref="UpdateProfileRequest"/>.
/// </summary>
public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfileRequestValidator"/> class.
    /// </summary>
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.LoginName)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(40).WithMessage("Name must not exceed 40 characters.");

        RuleFor(x => x.LoginMobileNo)
            .MaximumLength(15).WithMessage("Mobile number must not exceed 15 characters.");

        RuleFor(x => x.LoginEmailId)
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.LoginEmailId))
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.MenuId)
            .MaximumLength(8).WithMessage("Menu ID must not exceed 8 characters.");
    }
}
