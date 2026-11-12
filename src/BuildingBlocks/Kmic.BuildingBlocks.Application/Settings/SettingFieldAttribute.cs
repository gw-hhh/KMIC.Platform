namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 设置值里一个成员的界面说明：管理页按成员类型生成编辑控件，用它写显示名和单位
/// </summary>
/// <remarks>
/// 可以不写，不写时界面显示成员名。成员的类型决定控件，所以这里不再声明类型
/// </remarks>
/// <param name="displayName">显示名</param>
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
