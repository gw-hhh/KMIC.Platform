namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 声明一项系统设置：运行中可调、全实例一致生效的业务参数
/// </summary>
/// <param name="key">键，以本模块名开头，如 "Sample.Approval.Threshold"</param>
/// <param name="displayName">管理界面中的显示名</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemSettingAttribute(string key, string displayName) : Attribute
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
    /// 管理界面中的分组
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// 修改所需的权限点，必填
    /// </summary>
    public string EditPermission { get; init; } = string.Empty;
}
