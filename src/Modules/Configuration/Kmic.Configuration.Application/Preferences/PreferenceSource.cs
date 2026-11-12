using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 一个偏好当前生效的值来自哪里
/// </summary>
public sealed class PreferenceSource : Enumeration<PreferenceSource>
{
    /// <summary>
    /// 没有任何一层设置过，用代码里的默认值
    /// </summary>
    public static readonly PreferenceSource Default = new("Default", "默认");

    /// <summary>
    /// 系统层
    /// </summary>
    public static readonly PreferenceSource System = new("System", "系统");

    /// <summary>
    /// 角色层
    /// </summary>
    public static readonly PreferenceSource Role = new("Role", "角色");

    /// <summary>
    /// 本人在当前角色下设置的
    /// </summary>
    public static readonly PreferenceSource UserRole = new("UserRole", "本人");

    private PreferenceSource(string code, string name)
        : base(code, name)
    {
    }
}
