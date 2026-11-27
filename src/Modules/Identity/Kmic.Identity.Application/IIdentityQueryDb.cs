using Kmic.Identity.Application.SecurityEvents;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application;

public interface IIdentityQueryDb : IIdentityReads
{
    /// <summary>会话及刷新令牌哈希</summary>
    IQueryable<Session> Sessions { get; }

    /// <summary>安全事件只读投影（由框架写入）</summary>
    IQueryable<SecurityEventRow> SecurityEvents { get; }
}
