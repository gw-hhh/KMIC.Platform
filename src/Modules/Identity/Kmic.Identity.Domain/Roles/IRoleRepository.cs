using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色仓储
/// </summary>
public interface IRoleRepository : IRepository<Role, RoleId>
{
    /// <summary>
    /// 查询配置了指定权限中任一项的角色，包含权限和数据范围
    /// </summary>
    Task<IReadOnlyList<Role>> ListWithPermissionsAsync(IReadOnlyCollection<string> permissionCodes, CancellationToken ct);

    /// <summary>
    /// 查询对指定对象单独设置了数据范围的角色，包含权限和数据范围
    /// </summary>
    Task<IReadOnlyList<Role>> ListWithDataScopeOverridesAsync(IReadOnlyCollection<string> scopeObjects, CancellationToken ct);

    /// <summary>
    /// 查询拥有全部权限的角色，供目录同步使用
    /// </summary>
    Task<IReadOnlyList<Role>> ListGrantingAllAsync(CancellationToken ct);
}
