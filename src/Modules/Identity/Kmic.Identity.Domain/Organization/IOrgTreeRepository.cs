using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Organization;

/// <summary>
/// 组织树的仓储
/// </summary>
public interface IOrgTreeRepository : IRepository<OrgTree, OrgTreeId>;
