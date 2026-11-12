using FluentValidation;

namespace Kmic.Identity.Application.Authentication.Login;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Account).NotEmpty().MaximumLength(100);

        // 密码长度只设上限，防止超长输入拖慢哈希；具体长度规则在设置密码时检查，登录时不提示“密码太短”
        RuleFor(c => c.Password).NotEmpty().MaximumLength(1024);
        RuleFor(c => c.ClientType).Must(type => ClientKinds.TryParse(type, out _)).WithMessage("客户端类型只能是 web 或 app");
    }
}
