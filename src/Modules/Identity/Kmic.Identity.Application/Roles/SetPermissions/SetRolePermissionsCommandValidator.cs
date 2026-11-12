using FluentValidation;

namespace Kmic.Identity.Application.Roles.SetPermissions;

internal sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator() =>
        RuleFor(c => c.Permissions).NotNull().Must(p => p.Count <= 2000).WithMessage("权限点最多 2000 个");
}
