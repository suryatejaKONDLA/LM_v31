using FluentValidation;

namespace CITL.Application.Core.Admin.MailMaster;

/// <summary>
/// FluentValidation validator for <see cref="MailMasterRequest"/>.
/// </summary>
public sealed class MailMasterRequestValidator : AbstractValidator<MailMasterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MailMasterRequestValidator"/> class.
    /// </summary>
    public MailMasterRequestValidator()
    {
        RuleFor(x => x.MailBranchCode)
            .GreaterThan(0).WithMessage("Branch code must be greater than 0.");

        RuleFor(x => x.MailFromAddress)
            .NotEmpty().WithMessage("From address is required.")
            .MaximumLength(100).WithMessage("From address must not exceed 100 characters.")
            .EmailAddress().WithMessage("From address must be a valid email address.");

        RuleFor(x => x.MailFromPassword)
            .NotEmpty().WithMessage("Mail password is required.")
            .MaximumLength(256).WithMessage("Mail password must not exceed 256 characters.");

        RuleFor(x => x.MailDisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(40).WithMessage("Display name must not exceed 40 characters.");

        RuleFor(x => x.MailHost)
            .NotEmpty().WithMessage("Mail host is required.")
            .MaximumLength(40).WithMessage("Mail host must not exceed 40 characters.");

        RuleFor(x => x.MailPort)
            .GreaterThan(0).WithMessage("Port must be greater than 0.")
            .LessThanOrEqualTo(65535).WithMessage("Port must not exceed 65535.");

        RuleFor(x => x.MailMaxRecipients)
            .GreaterThan(0).WithMessage("Max recipients must be greater than 0.");

        RuleFor(x => x.MailRetryAttempts)
            .GreaterThanOrEqualTo(0).WithMessage("Retry attempts must be 0 or greater.");

        RuleFor(x => x.MailRetryIntervalMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Retry interval must be 0 or greater.");
    }
}
