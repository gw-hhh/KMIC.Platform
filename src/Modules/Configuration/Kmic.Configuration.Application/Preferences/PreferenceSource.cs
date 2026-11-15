using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 偏好生效值的来源
/// </summary>
public sealed class PreferenceSource : Enumeration<PreferenceSource>
{
    /// <summary>
    /// 所有允许层均无可用值时使用代码默认值
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
