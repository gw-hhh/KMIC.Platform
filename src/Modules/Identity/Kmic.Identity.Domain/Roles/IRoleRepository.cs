using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色的仓储
/// </summary>
public interface IRoleRepository : IRepository<Role, RoleId>;
