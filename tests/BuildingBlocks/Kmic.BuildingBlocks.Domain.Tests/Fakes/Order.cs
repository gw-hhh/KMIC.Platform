using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

/// <summary>
/// 测试用的聚合：按 05-01 示例的写法，有明细才能提交
/// </summary>
internal sealed class Order : AggregateRoot<OrderId>, IHasRevision
{
    // 测试需要直接调用 CanFire、PermittedActions，所以公开为属性；业务聚合中写成 private static readonly 字段 _machine
    public static StateMachine<Order, OrderStatus, OrderAction> Machine { get; } =
        StateMachine.Define<Order, OrderStatus, OrderAction>(o => o.Status, (o, v) => o.Status = v)
            .Initial(OrderStatus.Draft)
            .OnInvalid(OrderErrors.InvalidTransition)
            .Permit(OrderStatus.Draft, OrderAction.Submit, OrderStatus.Submitted, guard: o => o.ItemCount > 0 ? Result.Success() : OrderErrors.NoItems)
            .Permit(OrderStatus.Submitted, OrderAction.Approve, OrderStatus.Approved)
            .Permit(OrderStatus.Submitted, OrderAction.Reject, OrderStatus.Rejected)
            .Permit(OrderStatus.Rejected, OrderAction.Submit, OrderStatus.Submitted)
            .Build();

    private Order(OrderId id)
        : base(id)
    {
    }

    public OrderStatus Status { get; private set; } = null!;

    public int ItemCount { get; private set; }

    public int Revision { get; private set; } = 1;

    public static Order Create(string? reason = null)
    {
        var order = new Order(OrderId.New());
        Machine.Start(order, reason);
        order.Raise(new OrderCreated(order.Id));
        return order;
    }

    public void AddItem() => ItemCount++;

    public Result Submit(string? reason = null) => Machine.Fire(this, OrderAction.Submit, reason);

    public Result Approve()
    {
        var fired = Machine.Fire(this, OrderAction.Approve, null);
        if (fired.IsFailure)
        {
            return fired;
        }

        Raise(new OrderApproved(Id));
        return Result.Success();
    }
}
