using Kmic.BuildingBlocks.Domain.Events;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

internal sealed record OrderCreated(OrderId OrderId) : IDomainEvent;
