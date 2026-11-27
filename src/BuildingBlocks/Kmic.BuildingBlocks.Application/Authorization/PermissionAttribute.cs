namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明权限点元数据。</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class PermissionAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;

    /// <summary>是否属于高风险敏感操作（如物理删除、模拟登录、全量导出等）。</summary>
    public bool Sensitive { get; init; }

    /// <summary>改名前的旧编码，须待所有环境完成一次启动同步后方可移除。</summary>
    public string? RenamedFrom { get; init; }
}
