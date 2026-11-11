namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 权限点
/// </summary>
/// <param name="displayName">显示名称</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class PermissionAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 是否敏感
    /// </summary>
    /// <remarks>
    /// 物理删除、模拟登录、导出全部数据这类权限要标为敏感，用到它的命令必须审计
    /// </remarks>
    public bool Sensitive { get; init; }
}
