using System.Collections.Frozen;
using FluentValidation;

namespace CITL.Application.Core.Admin.Mappings.Mapping;

public sealed class MappingsRequestValidator : AbstractValidator<MappingsRequest>
{
    private static readonly FrozenSet<string> SupportedQueryStrings =
        FrozenSet.ToFrozenSet(["010703"]);

    public MappingsRequestValidator()
    {
        RuleFor(x => x.QueryString)
            .NotEmpty()
            .Must(SupportedQueryStrings.Contains)
            .WithMessage("Unsupported mapping type.");

        RuleFor(x => x.AnchorId)
            .NotEmpty()
            .WithMessage("Anchor ID is required.");
    }
}
