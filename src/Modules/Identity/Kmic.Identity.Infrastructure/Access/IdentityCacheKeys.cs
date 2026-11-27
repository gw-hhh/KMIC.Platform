using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Infrastructure.Access;

// 键中附带版本号，角色或组织树变更后自动切向新键
internal static class IdentityCacheKeys
{
    public static string RoleSnapshot(RoleId role, long version) => CacheKey.Of(IdentityModuleInfo.Name, "RoleSnapshot", role.Value, version);

    public static string OrgTree(long version) => CacheKey.Of(IdentityModuleInfo.Name, "OrgTree", version);
}
