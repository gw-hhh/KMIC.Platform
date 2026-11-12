using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Specimens.Events;

namespace Kmic.Sample.Application.Specimens.EventHandlers;

/// <summary>
/// 标本提交后通知其他模块：领域事件转成集成事件
/// </summary>
/// <remarks>
/// 和提交命令在同一个事务里，集成事件随业务数据一起写入 Outbox，提交之后才会投递
/// </remarks>
internal sealed class PublishSpecimenSubmitted(IIntegrationEventPublisher publisher) : IDomainEventHandler<SpecimenSubmitted>
{
    public ValueTask<Result> HandleAsync(SpecimenSubmitted domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        publisher.Add(new Contracts.Events.SpecimenSubmitted(domainEvent.Id.Value, domainEvent.Number));
        return ValueTask.FromResult(Result.Success());
    }
}
