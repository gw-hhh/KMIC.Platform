using Kmic.Identity.Application.SecurityEvents;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application;

/// <summary>
/// Identity 模块的查询入口
/// </summary>
public interface IIdentityQueryDb : IIdentityReads
{
    /// <summary>
    /// 会话及刷新令牌哈希
    /// </summary>
    IQueryable<Session> Sessions { get; }

    /// <summary>
    /// 安全事件的只读投影，写入由框架负责
    /// </summary>
    IQueryable<SecurityEventRow> SecurityEvents { get; }
}
