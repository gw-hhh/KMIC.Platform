using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[IntegrationMessage("BuildingBlocks.TicketClosedNotice")]
[OrderingKey(nameof(TicketId))]
internal sealed record TicketClosedNotice(Guid TicketId) : IIntegrationEvent;
