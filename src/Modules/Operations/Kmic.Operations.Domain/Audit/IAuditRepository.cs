using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Operations.Domain.Audit;

/// <summary>审计记录仓储（仅支持新增）。</summary>
public interface IAuditRepository : IRepository<AuditLog, AuditLogId>
{
    /// <summary>判断指定审计记录是否已存在，用于跳过重复投递的审计消息。</summary>
    Task<bool> ExistsAsync(AuditLogId id, CancellationToken ct);
}
