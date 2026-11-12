using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色的仓储
/// </summary>
public interface IRoleRepository : IRepository<Role, RoleId>
{
    /// <summary>
    /// 配置了其中任何一个权限点的角色，带权限点与数据范围，要修改它们时用
    /// </summary>
    Task<IReadOnlyList<Role>> ListWithPermissionsAsync(IReadOnlyCollection<string> permissionCodes, CancellationToken ct);

    /// <summary>
    /// 对其中任何一个范围对象单独设置了等级的角色，带权限点与数据范围，要修改它们时用
    /// </summary>
    Task<IReadOnlyList<Role>> ListWithDataScopeOverridesAsync(IReadOnlyCollection<string> scopeObjects, CancellationToken ct);

    /// <summary>
    /// 拥有全部权限点的角色，目录变化后要同步它们的指纹
    /// </summary>
    Task<IReadOnlyList<Role>> ListGrantingAllAsync(CancellationToken ct);
}
