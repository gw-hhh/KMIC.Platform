namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 标在模块的权限点常量类上，声明分组
/// </summary>
/// <param name="module">模块名，权限点编码的第一段必须与它相同</param>
/// <param name="displayName">角色管理界面中的分组名</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PermissionGroupAttribute(string module, string displayName) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;

    /// <summary>
    /// 分组显示名
    /// </summary>
    public string DisplayName { get; } = displayName;
}
