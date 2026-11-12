using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// Identity 模块的缓存键
/// </summary>
internal static class IdentityCacheKeys
{
    /// <summary>
    /// 角色授权快照，键里带版本号，内容不可变，不需要失效
    /// </summary>
    public static string RoleSnapshot(RoleId role, long version) => CacheKey.Of(IdentityModuleInfo.Name, "RoleSnapshot", role.Value, version);

    /// <summary>
    /// 组织树快照，键里带树版本号
    /// </summary>
    public static string OrgTree(long version) => CacheKey.Of(IdentityModuleInfo.Name, "OrgTree", version);
}
