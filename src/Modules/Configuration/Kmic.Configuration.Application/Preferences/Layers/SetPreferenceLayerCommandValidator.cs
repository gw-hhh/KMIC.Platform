using FluentValidation;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

internal sealed class SetPreferenceLayerCommandValidator : AbstractValidator<SetPreferenceLayerCommand>
{
    public SetPreferenceLayerCommandValidator() =>
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PreferenceValue.MaxKeyLength);
}
