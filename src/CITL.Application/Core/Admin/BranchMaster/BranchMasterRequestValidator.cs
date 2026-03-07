using FluentValidation;

namespace CITL.Application.Core.Admin.BranchMaster;

/// <summary>
/// Validates <see cref="BranchMasterRequest"/> fields.
/// </summary>
public sealed class BranchMasterRequestValidator : AbstractValidator<BranchMasterRequest>
{
    public BranchMasterRequestValidator()
    {
        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch Name is required.")
            .Length(1, 4).WithMessage("Branch Name must be between 1 and 4 characters.");

        RuleFor(x => x.BranchState)
            .GreaterThan(0).WithMessage("State is required.");

        RuleFor(x => x.BranchName2)
            .NotEmpty().WithMessage("Display Name is required.")
            .MaximumLength(40).WithMessage("Display Name must not exceed 40 characters.");

        RuleFor(x => x.BranchAddress1)
            .MaximumLength(40).WithMessage("Address Line 1 must not exceed 40 characters.")
            .When(x => x.BranchAddress1 is not null);

        RuleFor(x => x.BranchAddress2)
            .MaximumLength(40).WithMessage("Address Line 2 must not exceed 40 characters.")
            .When(x => x.BranchAddress2 is not null);

        RuleFor(x => x.BranchAddress3)
            .MaximumLength(40).WithMessage("Address Line 3 must not exceed 40 characters.")
            .When(x => x.BranchAddress3 is not null);

        RuleFor(x => x.BranchCity)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(40).WithMessage("City must not exceed 40 characters.");

        RuleFor(x => x.BranchPin)
            .NotEmpty().WithMessage("PIN Code is required.")
            .Length(6, 6).WithMessage("PIN Code must be exactly 6 characters.");

        RuleFor(x => x.BranchContactPerson)
            .NotEmpty().WithMessage("Contact Person is required.")
            .MaximumLength(40).WithMessage("Contact Person must not exceed 40 characters.");

        RuleFor(x => x.BranchPhoneNo1)
            .NotEmpty().WithMessage("Phone No 1 is required.")
            .MaximumLength(15).WithMessage("Phone No 1 must not exceed 15 characters.");

        RuleFor(x => x.BranchPhoneNo2)
            .MaximumLength(15).WithMessage("Phone No 2 must not exceed 15 characters.")
            .When(x => x.BranchPhoneNo2 is not null);

        RuleFor(x => x.BranchEmailId)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.BranchGstin)
            .MaximumLength(15).WithMessage("GSTIN must not exceed 15 characters.")
            .When(x => x.BranchGstin is not null);

        RuleFor(x => x.BranchPanNo)
            .MaximumLength(10).WithMessage("PAN No must not exceed 10 characters.")
            .When(x => x.BranchPanNo is not null);

        RuleFor(x => x.BranchCurrencyCode)
            .NotEmpty().WithMessage("Currency Code is required.")
            .Length(3, 3).WithMessage("Currency Code must be exactly 3 characters.");

        RuleFor(x => x.BranchTimeZoneCode)
            .GreaterThan(0).WithMessage("Time Zone is required.");

        RuleFor(x => x.BranchOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display Order must be 0 or greater.");
    }
}
