namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 声明一项偏好
/// </summary>
/// <param name="key">键，以本模块名开头，如 "Sample.List.PageSize"</param>
/// <param name="displayName">管理界面中的显示名</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PreferenceAttribute(string key, string displayName) : Attribute
{
    /// <summary>
    /// 键
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 允许设置的层
    /// </summary>
    public PreferenceLayers Layers { get; init; } = PreferenceLayers.All;
}
