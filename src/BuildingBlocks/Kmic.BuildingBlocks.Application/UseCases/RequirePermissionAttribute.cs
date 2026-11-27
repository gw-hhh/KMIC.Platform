namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明用例执行所需的权限点编码。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionAttribute(string permission) : Attribute
{
    public string Permission { get; } = permission;
}
