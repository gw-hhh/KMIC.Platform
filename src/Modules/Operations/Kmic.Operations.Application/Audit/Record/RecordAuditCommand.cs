using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;

namespace Kmic.Operations.Application.Audit.Record;

/// <summary>
/// 把一条审计消息落进审计汇总表
/// </summary>
/// <remarks>
/// 订阅 Framework.AuditRecorded，由收件箱以系统身份执行。按审计 ID 幂等：同一条消息重复投递只写一次。
/// 失败的重试与死信由框架处理，不影响产生审计的业务事务
/// </remarks>
/// <param name="Audit">审计消息</param>
[SystemOnly]
[NotAudited("审计汇总本身")]
public sealed record RecordAuditCommand(AuditRecorded Audit) : ICommand;
