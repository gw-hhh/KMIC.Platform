using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application;

/// <summary>
/// 查询与命令共用的身份数据读取接口
/// </summary>
public interface IIdentityReads
{
    /// <summary>
    /// 用户查询入口，授予和锚点由调用方按需查询或加载
    /// </summary>
    IQueryable<User> Users { get; }

    /// <summary>
    /// 角色查询入口，权限点和数据范围覆盖由调用方按需查询或加载
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// 组织树查询入口，组织单元由调用方按需查询或加载
    /// </summary>
    IQueryable<OrgTree> OrgTrees { get; }
}
