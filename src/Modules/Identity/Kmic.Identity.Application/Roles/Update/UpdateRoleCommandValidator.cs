using FluentValidation;

namespace Kmic.Identity.Application.Roles.Update;

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).MaximumLength(500);
        RuleFor(c => c.SortOrder).InclusiveBetween(0, 10_000);
    }
}
