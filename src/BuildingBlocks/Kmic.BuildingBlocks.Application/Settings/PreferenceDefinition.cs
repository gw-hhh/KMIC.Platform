using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好目录中的一项
/// </summary>
public sealed class PreferenceDefinition : DeclaredValueDefinition
{
    internal PreferenceDefinition(PreferenceAttribute declaration, Type valueType, object defaultValue, Func<object, Result> validate)
        : base(declaration.Key, declaration.DisplayName, valueType, defaultValue, validate) =>
        Layers = declaration.Layers;

    /// <summary>
    /// 允许设置的层
    /// </summary>
    public PreferenceLayers Layers { get; }
}
