using FluentValidation;

namespace Kmic.Identity.Application.Impersonation.Start;

internal sealed class StartImpersonationCommandValidator : AbstractValidator<StartImpersonationCommand>
{
    public StartImpersonationCommandValidator()
    {
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}
