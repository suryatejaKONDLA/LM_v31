using FluentValidation;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// FluentValidation validator for <see cref="LoginRequest"/>.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginRequestValidator"/> class.
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.LoginUser)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        RuleFor(x => x.LoginPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.");

        RuleFor(x => x.LoginIp)
            .MaximumLength(45).WithMessage("IP address must not exceed 45 characters.");

        RuleFor(x => x.LoginDevice)
            .MaximumLength(255).WithMessage("Device identifier must not exceed 255 characters.");

        // CAPTCHA fields — conditional: if one is provided, the other is required too
        RuleFor(x => x.CaptchaValue)
            .NotEmpty().WithMessage("CAPTCHA answer is required when CAPTCHA ID is provided.")
            .MaximumLength(20).WithMessage("CAPTCHA answer must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CaptchaId));

        RuleFor(x => x.CaptchaId)
            .NotEmpty().WithMessage("CAPTCHA ID is required when CAPTCHA answer is provided.")
            .MaximumLength(50).WithMessage("CAPTCHA ID must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CaptchaValue));
    }
}
