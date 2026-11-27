namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>声明系统设置。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemSettingAttribute(string key, string displayName) : Attribute
{
    public string Key { get; } = key;

    public string DisplayName { get; } = displayName;

    public string? Group { get; init; }

    public string EditPermission { get; init; } = string.Empty;
}
