using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 与命令同一事务：给单据加一行备注；测试可以让它失败，整个命令随之回滚
/// </summary>
internal sealed class TicketClosedHandler(TicketRepository tickets, ProbeHooks hooks) : IDomainEventHandler<TicketClosed>
{
    public async ValueTask<Result> HandleAsync(TicketClosed domainEvent, CancellationToken ct)
    {
        if (hooks.FailClosedEvent)
        {
            return TicketErrors.Rejected;
        }

        var loaded = await tickets.GetForUpdateAsync(domainEvent.Id, null, ct);
        loaded.Value.AddLine("已关闭");
        return Result.Success();
    }
}
