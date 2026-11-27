using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>偏好的存储层级</summary>
public sealed class PreferenceLayer : Enumeration<PreferenceLayer>
{
    /// <summary>全局共用，不关联角色或用户</summary>
    public static readonly PreferenceLayer System = new("System", "系统");

    /// <summary>同一角色的用户共用</summary>
    public static readonly PreferenceLayer Role = new("Role", "角色");

    /// <summary>按用户和角色分别保存</summary>
    public static readonly PreferenceLayer UserRole = new("UserRole", "本人");

    private PreferenceLayer(string code, string name)
        : base(code, name)
    {
    }
}
