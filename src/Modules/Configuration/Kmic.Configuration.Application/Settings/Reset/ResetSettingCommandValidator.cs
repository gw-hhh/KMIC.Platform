using FluentValidation;

namespace Kmic.Configuration.Application.Settings.Reset;

internal sealed class ResetSettingCommandValidator : AbstractValidator<ResetSettingCommand>
{
    public ResetSettingCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}
