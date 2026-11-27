using FluentValidation;

namespace Kmic.Identity.Application.Roles.SetDataScopes;

internal sealed class SetRoleDataScopesCommandValidator : AbstractValidator<SetRoleDataScopesCommand>
{
    public SetRoleDataScopesCommandValidator()
    {
        RuleFor(c => c.DefaultLevel).NotNull();
        RuleFor(c => c.Overrides).NotNull().Must(o => o.Count <= 500).WithMessage("单独设置的范围对象最多 500 个");
        // 字典值集合为 .NET 内置属性，无法自动提取 XML 注释中的显示名
        RuleForEach(c => c.Overrides.Values).NotNull().WithName("单独设置的等级");
    }
}
