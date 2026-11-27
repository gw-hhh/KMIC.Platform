namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明权限点分组的所属模块与显示名称。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PermissionGroupAttribute(string module, string displayName) : Attribute
{
    public string Module { get; } = module;

    public string DisplayName { get; } = displayName;
}
