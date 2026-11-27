namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>设置值成员的界面显示名与单位。</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SettingFieldAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;

    /// <summary>单位，如 天、小时、元。</summary>
    public string? Unit { get; init; }
}
