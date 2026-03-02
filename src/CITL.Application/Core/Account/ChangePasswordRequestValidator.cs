using FluentValidation;

namespace CITL.Application.Core.Account;

/// <summary>
/// FluentValidation validator for <see cref="ChangePasswordRequest"/>.
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordRequestValidator"/> class.
    /// </summary>
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.LoginPasswordOld)
            .NotEmpty().WithMessage("Current password is required.")
            .MaximumLength(25).WithMessage("Current password must not exceed 25 characters.");

        RuleFor(x => x.LoginPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MaximumLength(25).WithMessage("New password must not exceed 25 characters.");
    }
}
