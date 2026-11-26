using FluentValidation;

namespace Kmic.Identity.Application.Roles.SetDataScopes;

internal sealed class SetRoleDataScopesCommandValidator : AbstractValidator<SetRoleDataScopesCommand>
{
    public SetRoleDataScopesCommandValidator()
    {
        RuleFor(c => c.DefaultLevel).NotNull();
        RuleFor(c => c.Overrides).NotNull().Must(o => o.Count <= 500).WithMessage("单独设置的范围对象最多 500 个");
        // 字典的值集合是 .NET 自带的属性，取不到注释里的中文名
        RuleForEach(c => c.Overrides.Values).NotNull().WithName("单独设置的等级");
    }
}
