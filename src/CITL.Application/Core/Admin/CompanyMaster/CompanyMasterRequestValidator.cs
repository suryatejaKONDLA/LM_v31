using FluentValidation;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// FluentValidation validator for <see cref="CompanyMasterRequest"/>.
/// Enforces all business rules for the company master configuration.
/// </summary>
public sealed class CompanyMasterRequestValidator : AbstractValidator<CompanyMasterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyMasterRequestValidator"/> class.
    /// </summary>
    public CompanyMasterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Company full name is required.")
            .MaximumLength(150).WithMessage("Company full name must not exceed 150 characters.");

        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Company short name is required.")
            .MaximumLength(30).WithMessage("Company short name must not exceed 30 characters.");

        RuleFor(x => x.Mobile1)
            .MaximumLength(20).WithMessage("Mobile 1 must not exceed 20 characters.")
            .Matches(@"^[\d\+\-\s\(\)]+$").WithMessage("Mobile 1 must contain only digits, spaces, and phone characters (+, -, parentheses).")
            .When(x => x.Mobile1 is not null);

        RuleFor(x => x.Mobile2)
            .MaximumLength(20).WithMessage("Mobile 2 must not exceed 20 characters.")
            .Matches(@"^[\d\+\-\s\(\)]+$").WithMessage("Mobile 2 must contain only digits, spaces, and phone characters (+, -, parentheses).")
            .When(x => x.Mobile2 is not null);

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => x.Email is not null);

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website must not exceed 200 characters.")
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Website must be a valid URL.")
            .When(x => x.Website is not null);

        RuleFor(x => x.Tagline)
            .MaximumLength(100).WithMessage("Tagline must not exceed 100 characters.");

        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("Session ID is required.");

        RuleFor(x => x.BranchCode)
            .GreaterThan(0).WithMessage("Branch code is required.");
    }
}
