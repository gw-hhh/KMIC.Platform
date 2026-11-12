using FluentValidation;

namespace Kmic.Configuration.Application.Settings.Purge;

internal sealed class PurgeObsoleteSettingCommandValidator : AbstractValidator<PurgeObsoleteSettingCommand>
{
    public PurgeObsoleteSettingCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}
