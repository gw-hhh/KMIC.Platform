using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>
/// 偏好存在哪一层
/// </summary>
/// <remarks>
/// 读取时由下往上取第一个有值的：用户在当前角色下的设置、角色、系统，都没有就用代码里的默认值
/// </remarks>
public sealed class PreferenceLayer : Enumeration<PreferenceLayer>
{
    /// <summary>
    /// 系统层：对所有人生效，不属于任何角色或用户
    /// </summary>
    public static readonly PreferenceLayer System = new("System", "系统");

    /// <summary>
    /// 角色层：对这个角色下的所有人生效
    /// </summary>
    public static readonly PreferenceLayer Role = new("Role", "角色");

    /// <summary>
    /// 用户层：某个用户在某个角色下自己的设置，换了角色是另一份
    /// </summary>
    public static readonly PreferenceLayer UserRole = new("UserRole", "本人");

    private PreferenceLayer(string code, string name)
        : base(code, name)
    {
    }
}
