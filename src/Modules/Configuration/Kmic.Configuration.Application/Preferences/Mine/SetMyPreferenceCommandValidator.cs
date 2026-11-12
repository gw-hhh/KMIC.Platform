using FluentValidation;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Mine;

internal sealed class SetMyPreferenceCommandValidator : AbstractValidator<SetMyPreferenceCommand>
{
    public SetMyPreferenceCommandValidator() =>
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PreferenceValue.MaxKeyLength);
}
