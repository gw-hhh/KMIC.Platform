namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 设置值成员的界面说明：显示名和单位
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SettingFieldAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 单位，如 天、小时、元
    /// </summary>
    public string? Unit { get; init; }
}
