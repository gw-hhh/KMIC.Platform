using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[IntegrationMessage("BuildingBlocks.TicketChanged")]
internal sealed record TicketChanged(TicketSnapshot Ticket) : ISnapshotEvent
{
    public Guid SourceId => Ticket.Id;
}
