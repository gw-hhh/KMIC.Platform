using Kmic.Identity.Application.SecurityEvents;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application;

/// <summary>
/// Identity 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器通过此接口读取，不跟踪实体，也不读取命令中尚未保存的修改。
/// Identity 数据不声明数据范围，访问由用例的权限要求决定
/// </remarks>
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
