using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 单据关闭时加一行备注
/// </summary>
/// <remarks>
/// 和命令在同一个事务里执行，测试可以让它失败，验证整个命令回滚
/// </remarks>
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
