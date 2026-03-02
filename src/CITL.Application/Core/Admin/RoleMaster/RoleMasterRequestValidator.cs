using FluentValidation;

namespace CITL.Application.Core.Admin.RoleMaster;

/// <summary>
/// FluentValidation validator for <see cref="RoleMasterRequest"/>.
/// </summary>
public sealed class RoleMasterRequestValidator : AbstractValidator<RoleMasterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleMasterRequestValidator"/> class.
    /// </summary>
    public RoleMasterRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(40).WithMessage("Role name must not exceed 40 characters.");

        RuleFor(x => x.BranchCode)
            .GreaterThan(0).WithMessage("Branch code must be greater than 0.");
    }
}
