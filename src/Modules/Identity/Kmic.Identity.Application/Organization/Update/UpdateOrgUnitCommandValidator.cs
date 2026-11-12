using FluentValidation;

namespace Kmic.Identity.Application.Organization.Update;

internal sealed class UpdateOrgUnitCommandValidator : AbstractValidator<UpdateOrgUnitCommand>
{
    public UpdateOrgUnitCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).MaximumLength(50);
    }
}
