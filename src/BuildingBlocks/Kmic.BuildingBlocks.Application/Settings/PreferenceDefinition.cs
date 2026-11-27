using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

public sealed class PreferenceDefinition : DeclaredValueDefinition
{
    internal PreferenceDefinition(PreferenceAttribute declaration, Type valueType, object defaultValue, Func<object, Result> validate)
        : base(declaration.Key, declaration.DisplayName, valueType, defaultValue, validate) =>
        Layers = declaration.Layers;

    public PreferenceLayers Layers { get; }
}
