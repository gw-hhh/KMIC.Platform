namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>声明用户偏好设置。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PreferenceAttribute(string key, string displayName) : Attribute
{
    public string Key { get; } = key;

    public string DisplayName { get; } = displayName;

    /// <summary>允许配置该偏好的层级范围。</summary>
    public PreferenceLayers Layers { get; init; } = PreferenceLayers.All;
}
