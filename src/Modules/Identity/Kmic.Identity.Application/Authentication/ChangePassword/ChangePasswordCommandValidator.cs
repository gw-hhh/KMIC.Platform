using FluentValidation;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // 具体长度规则由密码策略统一校验，此处仅限制上限以防超长输入拖慢哈希计算
        RuleFor(c => c.NewPassword).NotEmpty().MaximumLength(1024);
        RuleFor(c => c.OldPassword).MaximumLength(1024);
    }
}
