using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色拥有的一个权限点（条目）
/// </summary>
public sealed class RolePermissionEntry : Entity<RolePermissionEntryId>
{
    private RolePermissionEntry(RolePermissionEntryId id, string permissionCode)
        : base(id) => PermissionCode = permissionCode;

    /// <summary>
    /// 权限点编码，如 Sample.Specimen.View
    /// </summary>
    public string PermissionCode { get; private set; }

    internal static RolePermissionEntry Create(string permissionCode) => new(RolePermissionEntryId.New(), permissionCode);
}
