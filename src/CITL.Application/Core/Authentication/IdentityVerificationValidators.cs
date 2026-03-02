using FluentValidation;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// FluentValidation validator for <see cref="ForgotPasswordRequest"/>.
/// </summary>
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordRequestValidator"/> class.
    /// </summary>
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.LoginUser)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.LoginEmailId)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.LoginMobileNo)
            .NotEmpty().WithMessage("Mobile number is required.")
            .MaximumLength(15).WithMessage("Mobile number must not exceed 15 characters.");
    }
}

/// <summary>
/// FluentValidation validator for <see cref="ResetPasswordRequest"/>.
/// </summary>
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordRequestValidator"/> class.
    /// </summary>
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.")
            .MaximumLength(100).WithMessage("Token must not exceed 100 characters.");

        RuleFor(x => x.LoginPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MaximumLength(25).WithMessage("Password must not exceed 25 characters.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}

/// <summary>
/// FluentValidation validator for <see cref="ResendVerificationRequest"/>.
/// </summary>
public sealed class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResendVerificationRequestValidator"/> class.
    /// </summary>
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.LoginUser)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.LoginEmailId)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.LoginMobileNo)
            .NotEmpty().WithMessage("Mobile number is required.")
            .MaximumLength(15).WithMessage("Mobile number must not exceed 15 characters.");
    }
}

/// <summary>
/// FluentValidation validator for <see cref="VerifyEmailRequest"/>.
/// </summary>
public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyEmailRequestValidator"/> class.
    /// </summary>
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(100).WithMessage("Token must not exceed 100 characters.");
    }
}
