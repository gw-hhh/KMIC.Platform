using Kmic.BuildingBlocks.Domain.Events;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed record TicketClosed(TicketId Id) : IDomainEvent;
