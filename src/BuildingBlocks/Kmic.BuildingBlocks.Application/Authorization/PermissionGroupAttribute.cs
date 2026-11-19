namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 权限点分组
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PermissionGroupAttribute(string module, string displayName) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;

    /// <summary>
    /// 分组名称
    /// </summary>
    public string DisplayName { get; } = displayName;
}
