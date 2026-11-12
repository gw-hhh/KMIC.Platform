using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Operations.Domain.Audit;

/// <summary>
/// 审计记录的仓储：只增加
/// </summary>
public interface IAuditRepository : IRepository<AuditLog, AuditLogId>
{
    /// <summary>
    /// 这条审计是不是已经落过了：消息重复投递时据此跳过
    /// </summary>
    Task<bool> ExistsAsync(AuditLogId id, CancellationToken ct);
}
