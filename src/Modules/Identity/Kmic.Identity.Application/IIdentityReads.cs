using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application;

/// <summary>查询与命令共用的身份数据只读入口</summary>
public interface IIdentityReads
{
    /// <summary>用户查询入口（按需加载授予与锚点）</summary>
    IQueryable<User> Users { get; }

    /// <summary>角色查询入口（按需加载权限点与数据范围覆盖）</summary>
    IQueryable<Role> Roles { get; }

    /// <summary>组织树查询入口（按需加载组织单元）</summary>
    IQueryable<OrgTree> OrgTrees { get; }
}
