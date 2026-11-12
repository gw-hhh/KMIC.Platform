using FluentValidation;

namespace Kmic.Identity.Application.Users.Grants.GrantRole;

internal sealed class GrantRoleCommandValidator : AbstractValidator<GrantRoleCommand>
{
    public GrantRoleCommandValidator()
    {
        RuleFor(c => c.Anchors).NotNull().Must(a => a.Count <= 200).WithMessage("锚点最多 200 个");
    }
}
