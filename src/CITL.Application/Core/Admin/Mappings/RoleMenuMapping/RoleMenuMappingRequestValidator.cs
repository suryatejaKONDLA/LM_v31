using FluentValidation;

namespace CITL.Application.Core.Admin.Mappings.RoleMenuMapping;

public sealed class RoleMenuMappingRequestValidator : AbstractValidator<RoleMenuMappingRequest>
{
    public RoleMenuMappingRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage("Role is required.");

        RuleFor(x => x.MenuIds)
            .NotNull()
            .WithMessage("Menu selection cannot be null.");
    }
}
