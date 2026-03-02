using FluentValidation;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// FluentValidation validator for <see cref="AppMasterRequest"/>.
/// Enforces all business rules for the application master configuration.
/// </summary>
public sealed class AppMasterRequestValidator : AbstractValidator<AppMasterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppMasterRequestValidator"/> class.
    /// </summary>
    public AppMasterRequestValidator()
    {
        RuleFor(x => x.AppHeader1)
            .NotEmpty().WithMessage("Application header 1 is required.")
            .MaximumLength(60).WithMessage("Application header 1 must not exceed 60 characters.");

        RuleFor(x => x.AppHeader2)
            .NotEmpty().WithMessage("Application header 2 is required.")
            .MaximumLength(7).WithMessage("Application header 2 must not exceed 7 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Application header 2 must contain only letters and numbers (no spaces or symbols).");

        RuleFor(x => x.AppLink)
            .MaximumLength(500).WithMessage("Application link must not exceed 500 characters.")
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Application link must be a valid URL.")
            .When(x => x.AppLink is not null);

        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("Session ID is required.");

        RuleFor(x => x.BranchCode)
            .GreaterThan(0).WithMessage("Branch code is required.");
    }
}
