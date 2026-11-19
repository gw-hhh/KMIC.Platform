namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 权限点
/// </summary>
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
    /// 物理删除、模拟登录、全量导出这类要标
    /// </remarks>
    public bool Sensitive { get; init; }

    /// <summary>
    /// 改名前的编码
    /// </summary>
    /// <remarks>
    /// 所有环境都启动同步过一次后才能删
    /// </remarks>
    public string? RenamedFrom { get; init; }
}
