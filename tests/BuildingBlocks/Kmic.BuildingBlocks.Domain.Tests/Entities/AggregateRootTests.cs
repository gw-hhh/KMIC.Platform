using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Tests.Fakes;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.Entities;

public sealed class AggregateRootTests
{
    [Fact]
    public void Raise_KeepsEventsUntilCleared()
    {
        var order = Order.Create();
        order.AddItem();
        order.Submit().IsSuccess.ShouldBeTrue();
        order.Approve().IsSuccess.ShouldBeTrue();

        order.DomainEvents.ShouldBe([new OrderCreated(order.Id), new OrderApproved(order.Id)]);
    }

    [Fact]
    public void ClearPendingRecords_ClearsEventsOnly()
    {
        var order = Order.Create();

        order.ClearPendingRecords();

        order.DomainEvents.ShouldBeEmpty();
        order.StateTransitions.Count.ShouldBe(1);
    }

    [Fact]
    public void ClearStateTransitions_ClearsTransitionsOnly()
    {
        var order = Order.Create();

        order.ClearStateTransitions();

        order.StateTransitions.ShouldBeEmpty();
        order.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void ConcurrencyVersion_HasNoPublicSetter()
    {
        var property = typeof(AggregateRoot<OrderId>).GetProperty(nameof(IAggregateRoot.ConcurrencyVersion))!;

        property.SetMethod.ShouldNotBeNull();
        property.SetMethod.IsPublic.ShouldBeFalse();
    }
}
