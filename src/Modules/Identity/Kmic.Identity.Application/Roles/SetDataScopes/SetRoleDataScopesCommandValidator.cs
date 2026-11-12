using FluentValidation;

namespace Kmic.Identity.Application.Roles.SetDataScopes;

internal sealed class SetRoleDataScopesCommandValidator : AbstractValidator<SetRoleDataScopesCommand>
{
    public SetRoleDataScopesCommandValidator()
    {
        RuleFor(c => c.DefaultLevel).IsInEnum();
        RuleFor(c => c.Overrides).NotNull().Must(o => o.Count <= 500).WithMessage("单独设置的范围对象最多 500 个");
        RuleForEach(c => c.Overrides.Values).IsInEnum();
    }
}
