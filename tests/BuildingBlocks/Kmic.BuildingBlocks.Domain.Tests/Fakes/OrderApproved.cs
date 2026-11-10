using Kmic.BuildingBlocks.Domain.Events;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

internal sealed record OrderApproved(OrderId OrderId) : IDomainEvent;
