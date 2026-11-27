namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>偏好配置层级，读取优先级为：UserRole &gt; Role &gt; System &gt; 默认值。</summary>
[Flags]
public enum PreferenceLayers
{
    /// <summary>无（声明时禁止使用）。</summary>
    None = 0,

    System = 1,

    Role = 2,

    /// <summary>用户在角色下的个人设置。</summary>
    UserRole = 4,

    All = System | Role | UserRole,
}
