using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Kmic.Sample.Domain.Specimens.Events;

namespace Kmic.Sample.Application.Specimens.EventHandlers;

/// <summary>
/// 标本提交后通知负责人：演示业务模块怎样发通知
/// </summary>
/// <remarks>
/// 和提交命令在同一个事务里，发送请求随业务数据一起写入 Outbox：事务回滚就不发，提交了才会投递。
/// 去重键带上标本 ID，同一个标本的提交通知不会因为重试发两次
/// </remarks>
internal sealed class NotifyOwnerOfSubmission(IIntegrationEventPublisher publisher) : IDomainEventHandler<SpecimenSubmitted>
{
    public ValueTask<Result> HandleAsync(SpecimenSubmitted domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var notice = new SpecimenSubmittedNotice(domainEvent.Number, domainEvent.Title, $"/sample/specimens/{domainEvent.Id.Value}");
        publisher.Send(SendNotification.For(notice, [Recipient.OfUser(domainEvent.OwnerId.Value)], $"Sample.SpecimenSubmitted:{domainEvent.Id.Value}"));
        return ValueTask.FromResult(Result.Success());
    }
}
