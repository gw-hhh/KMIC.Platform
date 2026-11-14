using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Operations.Domain.Audit;

/// <summary>
/// 审计记录的仓储：只增加
/// </summary>
public interface IAuditRepository : IRepository<AuditLog, AuditLogId>
{
    /// <summary>
    /// 用于跳过重复投递的审计消息
    /// </summary>
    Task<bool> ExistsAsync(AuditLogId id, CancellationToken ct);
}
