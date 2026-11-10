namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 标在权限点常量上，声明显示名与是否敏感；编码就是常量的值，格式“模块.资源.动作”
/// </summary>
/// <param name="displayName">角色管理界面中的显示名</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class PermissionAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 是否敏感：物理删除、模拟身份、导出全部数据类必须标记；角色管理界面单独提示，用到它的命令必须审计
    /// </summary>
    public bool Sensitive { get; init; }
}
