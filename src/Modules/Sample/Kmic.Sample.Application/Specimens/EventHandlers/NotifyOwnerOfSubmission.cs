using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Kmic.Sample.Domain.Specimens.Events;

namespace Kmic.Sample.Application.Specimens.EventHandlers;

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
