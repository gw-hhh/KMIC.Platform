using FluentValidation;

namespace Kmic.Identity.Application.Organization.Create;

internal sealed class CreateOrgUnitCommandValidator : AbstractValidator<CreateOrgUnitCommand>
{
    public CreateOrgUnitCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).MaximumLength(50);
    }
}
