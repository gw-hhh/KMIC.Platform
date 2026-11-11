namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好层级
/// </summary>
/// <remarks>
/// 读取顺序：用户在角色下的设置、角色、系统、默认值
/// </remarks>
[Flags]
public enum PreferenceLayers
{
    /// <summary>
    /// 无，声明时不能用
    /// </summary>
    None = 0,

    /// <summary>
    /// 系统
    /// </summary>
    System = 1,

    /// <summary>
    /// 角色
    /// </summary>
    Role = 2,

    /// <summary>
    /// 用户在角色下的设置
    /// </summary>
    UserRole = 4,

    /// <summary>
    /// 全部层
    /// </summary>
    All = System | Role | UserRole,
}
