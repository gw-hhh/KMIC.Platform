using Kmic.Identity.Application.SecurityEvents;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application;

/// <summary>
/// Identity 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：不跟踪实体，也不会读到命令里还没保存的修改。Identity 的数据都不声明数据范围，
/// 谁能看由权限点决定
/// </remarks>
public interface IIdentityQueryDb : IIdentityReads
{
    /// <summary>
    /// 会话，含刷新令牌
    /// </summary>
    IQueryable<Session> Sessions { get; }

    /// <summary>
    /// 安全事件：表由框架的写入器写，这里只读，用原生 SQL 映射到只读的行类型
    /// </summary>
    IQueryable<SecurityEventRow> SecurityEvents { get; }
}
