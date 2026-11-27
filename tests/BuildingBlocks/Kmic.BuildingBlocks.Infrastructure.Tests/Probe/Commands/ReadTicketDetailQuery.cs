using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 读取单据标题的敏感查询，执行成功后记录读审计日志。
/// </summary>
/// <param name="Id">单据</param>
/// <param name="TooManyDetails">附加超长说明使审计事件超出 Outbox 载荷上限，用于测试写审计失败场景</param>
[AllowAuthenticated]
[AuditedRead]
internal sealed record ReadTicketDetailQuery(TicketId Id, bool TooManyDetails = false) : IQuery<string>;
