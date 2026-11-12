using FluentValidation;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // 长度规则由密码策略检查并给出统一的错误；这里只防超长输入拖慢哈希
        RuleFor(c => c.NewPassword).NotEmpty().MaximumLength(1024);
        RuleFor(c => c.OldPassword).MaximumLength(1024);
    }
}
