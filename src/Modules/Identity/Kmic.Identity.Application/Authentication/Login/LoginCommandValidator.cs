using FluentValidation;

namespace Kmic.Identity.Application.Authentication.Login;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Account).NotEmpty().MaximumLength(100);

        // 仅限制密码长度上限以防超长输入拖慢哈希；下限在设密时校验，登录时不暴露长度规则
        RuleFor(c => c.Password).NotEmpty().MaximumLength(1024);
        RuleFor(c => c.ClientType).Must(type => ClientKinds.TryParse(type, out _)).WithMessage("客户端类型只能是 web 或 app");
    }
}
