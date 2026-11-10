namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好允许设置的层；解析顺序为 用户 + 角色 → 角色 → 系统 → 代码默认值
/// </summary>
[Flags]
public enum PreferenceLayers
{
    /// <summary>
    /// 不允许任何层，声明时不能使用
    /// </summary>
    None = 0,

    /// <summary>
    /// 系统层
    /// </summary>
    System = 1,

    /// <summary>
    /// 角色层
    /// </summary>
    Role = 2,

    /// <summary>
    /// 用户在某个角色下的设置
    /// </summary>
    UserRole = 4,

    /// <summary>
    /// 全部层
    /// </summary>
    All = System | Role | UserRole,
}
