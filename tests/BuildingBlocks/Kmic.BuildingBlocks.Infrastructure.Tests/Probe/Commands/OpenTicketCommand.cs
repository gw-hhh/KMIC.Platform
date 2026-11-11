using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 新建单据；标题为 Reject 时处理器返回业务失败
/// </summary>
[AllowAuthenticated]
[Idempotent]
[Audited]
internal sealed record OpenTicketCommand(string Number, string Title) : ICommand<CreatedResponse<TicketId>>
{
    public const string RejectedTitle = "Reject";
}
