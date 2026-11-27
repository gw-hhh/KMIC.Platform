using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

// 单据关闭时与命令同事务追加备注行并发布集成事件，便于测试事务回滚
internal sealed class TicketClosedHandler(TicketRepository tickets, IIntegrationEventPublisher publisher, ProbeHooks hooks) : IDomainEventHandler<TicketClosed>
{
    public async ValueTask<Result> HandleAsync(TicketClosed domainEvent, CancellationToken ct)
    {
        if (hooks.FailClosedEvent)
        {
            return TicketErrors.Rejected;
        }

        var loaded = await tickets.GetForUpdateAsync(domainEvent.Id, null, ct);
        loaded.Value.AddLine("已关闭");
        publisher.Add(new TicketClosedNotice(domainEvent.Id.Value));
        return Result.Success();
    }
}
