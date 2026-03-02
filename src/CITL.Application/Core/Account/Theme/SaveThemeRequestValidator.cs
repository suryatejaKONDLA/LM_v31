using FluentValidation;

namespace CITL.Application.Core.Account.Theme;

/// <summary>
/// FluentValidation validator for <see cref="SaveThemeRequest"/>.
/// </summary>
public sealed class SaveThemeRequestValidator : AbstractValidator<SaveThemeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveThemeRequestValidator"/> class.
    /// </summary>
    public SaveThemeRequestValidator()
    {
        RuleFor(x => x.ThemeJson)
            .NotEmpty().WithMessage("Theme JSON is required.");
    }
}
