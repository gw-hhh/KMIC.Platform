using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;

namespace Kmic.Operations.Application.Audit.Record;

/// <summary>
/// 把审计消息写入汇总表，同一审计 ID 只写一次
/// </summary>
[SystemOnly]
[NotAudited("仅汇总已有审计消息，避免再次生成审计记录")]
public sealed record RecordAuditCommand(AuditRecorded Audit) : ICommand;
