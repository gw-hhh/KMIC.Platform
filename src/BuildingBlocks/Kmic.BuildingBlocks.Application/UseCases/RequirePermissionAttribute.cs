namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要当前角色具有某个权限点（授权第 1 层）
/// </summary>
/// <param name="permission">权限点编码，格式为“模块.资源.动作”，取自模块的权限点常量</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionAttribute(string permission) : Attribute
{
    /// <summary>
    /// 权限点编码
    /// </summary>
    public string Permission { get; } = permission;
}
