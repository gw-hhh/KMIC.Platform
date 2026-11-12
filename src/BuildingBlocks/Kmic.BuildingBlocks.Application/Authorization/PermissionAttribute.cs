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

    /// <summary>
    /// 改名前的编码
    /// </summary>
    /// <remarks>
    /// 在新编码的常量上写旧编码，启动同步时把已有角色配置里的旧编码原地改成新编码，角色上的配置保留。
    /// 所有环境都同步过一次之后可以删掉
    /// </remarks>
    public string? RenamedFrom { get; init; }
}
