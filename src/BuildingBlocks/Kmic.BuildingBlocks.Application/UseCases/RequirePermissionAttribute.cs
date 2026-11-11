namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要权限点
/// </summary>
/// <param name="permission">权限点编码</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionAttribute(string permission) : Attribute
{
    /// <summary>
    /// 权限点编码
    /// </summary>
    public string Permission { get; } = permission;
}
