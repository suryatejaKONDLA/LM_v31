using FluentValidation;

namespace CITL.Application.Core.Notifications.Email;

/// <summary>
/// FluentValidation validator for <see cref="SendEmailRequest"/>.
/// </summary>
public sealed class SendEmailRequestValidator : AbstractValidator<SendEmailRequest>
{
    private const int MaxSubjectLength = 200;
    private const int MaxBodyLength = 50000;
    private const int MaxAddressFieldLength = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendEmailRequestValidator"/> class.
    /// </summary>
    public SendEmailRequestValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("At least one recipient is required.")
            .MaximumLength(MaxAddressFieldLength).WithMessage($"To field must not exceed {MaxAddressFieldLength} characters.")
            .Must(BeValidEmailList).WithMessage("To field contains an invalid email address.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(MaxSubjectLength).WithMessage($"Subject must not exceed {MaxSubjectLength} characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.")
            .MaximumLength(MaxBodyLength).WithMessage($"Body must not exceed {MaxBodyLength} characters.");

        RuleFor(x => x.Cc)
            .MaximumLength(MaxAddressFieldLength).WithMessage($"CC field must not exceed {MaxAddressFieldLength} characters.")
            .Must(BeValidEmailList).WithMessage("CC field contains an invalid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cc));

        RuleFor(x => x.Bcc)
            .MaximumLength(MaxAddressFieldLength).WithMessage($"BCC field must not exceed {MaxAddressFieldLength} characters.")
            .Must(BeValidEmailList).WithMessage("BCC field contains an invalid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bcc));

        RuleFor(x => x.MailSNo)
            .GreaterThan(0).WithMessage("Mail configuration ID must be greater than 0.")
            .When(x => x.MailSNo.HasValue);
    }

    private static bool BeValidEmailList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var addresses = value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (addresses.Length == 0)
        {
            return false;
        }

        foreach (var address in addresses)
        {
            if (!IsValidEmail(address))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var atIndex = email.IndexOf('@', StringComparison.Ordinal);
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return false;
        }

        var dotIndex = email.LastIndexOf('.', email.Length - 1, email.Length - atIndex - 1);
        return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
    }
}
