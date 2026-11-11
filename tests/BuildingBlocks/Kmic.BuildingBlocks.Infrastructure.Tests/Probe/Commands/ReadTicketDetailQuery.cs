using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 读单据标题的敏感查询，成功后写读审计
/// </summary>
/// <param name="Id">单据</param>
/// <param name="TooManyDetails">补充大量说明，让审计事件超过 Outbox 的上限，用来测写审计失败</param>
[AllowAuthenticated]
[AuditedRead]
internal sealed record ReadTicketDetailQuery(TicketId Id, bool TooManyDetails = false) : IQuery<string>;
