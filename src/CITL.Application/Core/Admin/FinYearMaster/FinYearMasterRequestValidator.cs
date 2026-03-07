using FluentValidation;

namespace CITL.Application.Core.Admin.FinYearMaster;

/// <summary>
/// Validates <see cref="FinYearMasterRequest"/>.
/// </summary>
public sealed class FinYearMasterRequestValidator : AbstractValidator<FinYearMasterRequest>
{
    public FinYearMasterRequestValidator()
    {
        RuleFor(x => x.FinYear)
            .GreaterThan(0).WithMessage("Financial Year is required.");

        RuleFor(x => x.FinDate1)
            .NotEmpty().WithMessage("Start Date is required.");

        RuleFor(x => x.FinDate2)
            .NotEmpty().WithMessage("End Date is required.")
            .GreaterThan(x => x.FinDate1).WithMessage("End Date must be after Start Date.");
    }
}
