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
    /// 安全事件：表由框架的写入器写，这里只读，用原生 SQL 映射到只读的行类型
    /// </summary>
    IQueryable<SecurityEventRow> SecurityEvents { get; }
}
