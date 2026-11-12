using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application;

/// <summary>
/// 按条件读用户、角色、组织树的入口：查询和命令里共用的判断逻辑只依赖它
/// </summary>
/// <remarks>
/// 不跟踪实体。读到的是哪一侧的数据由实现决定：<see cref="IIdentityQueryDb" /> 是查询侧（已提交的数据），
/// <see cref="IIdentityCommandReads" /> 是命令侧（和仓储同一个事务）
/// </remarks>
public interface IIdentityReads
{
    /// <summary>
    /// 用户，含授予与锚点
    /// </summary>
    IQueryable<User> Users { get; }

    /// <summary>
    /// 角色，含权限点与数据范围覆盖
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// 组织树，含全部单元
    /// </summary>
    IQueryable<OrgTree> OrgTrees { get; }
}
