using FluentValidation;

namespace Kmic.Identity.Application.Roles.Create;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).MaximumLength(500);
        RuleFor(c => c.SortOrder).InclusiveBetween(0, 10_000);
        RuleFor(c => c.DefaultScopeLevel).IsInEnum();
    }
}
