namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 系统设置声明
/// </summary>
/// <param name="key">键，如 "Sample.Approval.Threshold"</param>
/// <param name="displayName">显示名称</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemSettingAttribute(string key, string displayName) : Attribute
{
    /// <summary>
    /// 键
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 分组
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// 修改所需的权限点
    /// </summary>
    public string EditPermission { get; init; } = string.Empty;
}
