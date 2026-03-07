using FluentValidation;

namespace CITL.Application.Core.Admin.LoginMaster;

/// <summary>
/// Validates <see cref="LoginMasterRequest"/> before persistence.
/// </summary>
public sealed class LoginMasterRequestValidator : AbstractValidator<LoginMasterRequest>
{
    public LoginMasterRequestValidator()
    {
        RuleFor(x => x.LoginUser)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(100)
            .Matches(@"^\S+$").WithMessage("Login User must not contain spaces.")
            .When(x => x.LoginId == 0); // only required on insert

        RuleFor(x => x.LoginName)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(40);

        RuleFor(x => x.LoginDesignation)
            .NotEmpty()
            .MaximumLength(40);

        RuleFor(x => x.LoginMobileNo)
            .NotEmpty()
            .MaximumLength(15)
            .Matches(@"^\d{10}$").WithMessage("Mobile number must be 10 digits.");

        RuleFor(x => x.LoginEmailId)
            .NotEmpty()
            .MaximumLength(100)
            .EmailAddress();

        RuleFor(x => x.LoginGender)
            .NotEmpty()
            .MaximumLength(1);
    }
}
