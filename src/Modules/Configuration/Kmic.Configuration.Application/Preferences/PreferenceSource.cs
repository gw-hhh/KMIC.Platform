using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>偏好生效值的来源</summary>
public sealed class PreferenceSource : Enumeration<PreferenceSource>
{
    /// <summary>所有允许层均无可用值时回退代码默认值</summary>
    public static readonly PreferenceSource Default = new("Default", "默认");

    public static readonly PreferenceSource System = new("System", "系统");

    public static readonly PreferenceSource Role = new("Role", "角色");

    /// <summary>当前用户在当前角色下设置的值</summary>
    public static readonly PreferenceSource UserRole = new("UserRole", "本人");

    private PreferenceSource(string code, string name)
        : base(code, name)
    {
    }
}
