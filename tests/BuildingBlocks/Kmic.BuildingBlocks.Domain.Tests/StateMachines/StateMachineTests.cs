using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.BuildingBlocks.Domain.Tests.Fakes;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.StateMachines;

public sealed class StateMachineTests
{
    [Fact]
    public void Start_SetsInitialAndRecordsTransitionWithoutFromCode()
    {
        var order = Order.Create("新建");

        order.Status.ShouldBe(OrderStatus.Draft);
        order.StateTransitions.ShouldBe([new StateTransition("Status", null, "Draft", StateTransition.CreateActionCode, "新建", 1)]);
    }

    [Fact]
    public void Fire_WhenPermitted_ChangesStateAndRecords()
    {
        var order = Order.Create();
        order.AddItem();
        order.ClearStateTransitions();

        var result = order.Submit("请审批");

        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Submitted);
        order.StateTransitions.ShouldBe([new StateTransition("Status", "Draft", "Submitted", "Submit", "请审批", 1)]);
    }

    [Fact]
    public void Fire_WhenNotPermitted_ReturnsOnInvalidAndKeepsState()
    {
        var order = Order.Create();
        order.ClearStateTransitions();

        var result = order.Approve();

        result.Error.ShouldBeSameAs(OrderErrors.InvalidTransition);
        order.Status.ShouldBe(OrderStatus.Draft);
        order.StateTransitions.ShouldBeEmpty();
        order.DomainEvents.ShouldNotContain(e => e is OrderApproved);
    }

    [Fact]
    public void Fire_WhenGuardFails_ReturnsGuardErrorAndDoesNotRecord()
    {
        var order = Order.Create();
        order.ClearStateTransitions();

        var result = order.Submit();

        result.Error.ShouldBeSameAs(OrderErrors.NoItems);
        order.Status.ShouldBe(OrderStatus.Draft);
        order.StateTransitions.ShouldBeEmpty();
    }

    [Fact]
    public void CanFire_DoesNotChangeState()
    {
        var order = Order.Create();
        order.AddItem();

        Order.Machine.CanFire(order, OrderAction.Submit).IsSuccess.ShouldBeTrue();
        Order.Machine.CanFire(order, OrderAction.Approve).Error.ShouldBeSameAs(OrderErrors.InvalidTransition);
        order.Status.ShouldBe(OrderStatus.Draft);
    }

    [Fact]
    public void PermittedActions_OnlyDeclaredAndGuardPassing()
    {
        var order = Order.Create();
        Order.Machine.PermittedActions(order).ShouldBeEmpty();

        order.AddItem();
        Order.Machine.PermittedActions(order).ShouldBe([OrderAction.Submit]);

        order.Submit();
        Order.Machine.PermittedActions(order).ShouldBe([OrderAction.Approve, OrderAction.Reject]);
    }

    [Fact]
    public void ToMermaid_ListsInitialAndTransitions()
    {
        Order.Machine.ToMermaid().ShouldBe(
            "stateDiagram-v2\n"
            + "    [*] --> Draft\n"
            + "    Draft --> Submitted : Submit\n"
            + "    Submitted --> Approved : Approve\n"
            + "    Submitted --> Rejected : Reject\n"
            + "    Rejected --> Submitted : Submit\n");
    }

    [Fact]
    public void Build_WhenDeclarationDuplicated_Throws()
    {
        var builder = Define()
            .Initial(Light.Off)
            .OnInvalid(_invalid)
            .Permit(Light.Off, Switch.Toggle, Light.On)
            .Permit(Light.Off, Switch.Toggle, Light.Off);

        Should.Throw<InvalidOperationException>(builder.Build).Message.ShouldContain("重复");
    }

    [Fact]
    public void Build_WhenStateUnreachable_Throws()
    {
        var builder = Define()
            .Initial(Light.Off)
            .OnInvalid(_invalid);

        Should.Throw<InvalidOperationException>(builder.Build).Message.ShouldContain("On");
    }

    [Fact]
    public void Build_WhenOnInvalidOrInitialMissing_Throws()
    {
        Should.Throw<InvalidOperationException>(Define().Initial(Light.Off).Permit(Light.Off, Switch.Toggle, Light.On).Build)
            .Message.ShouldContain("OnInvalid");
        Should.Throw<InvalidOperationException>(Define().OnInvalid(_invalid).Permit(Light.Off, Switch.Toggle, Light.On).Build)
            .Message.ShouldContain("Initial");
    }

    [Fact]
    public void Define_WhenStateIsNotEnumeration_Throws()
    {
        Should.Throw<NotSupportedException>(Lamp.DefineByText);
    }

    [Fact]
    public void Fire_PlainEnum_RecordsMemberName()
    {
        var machine = Define()
            .Initial(Light.Off)
            .OnInvalid(_invalid)
            .Permit(Light.Off, Switch.Toggle, Light.On)
            .Permit(Light.On, Switch.Toggle, Light.Off)
            .Build();
        var lamp = new Lamp();
        machine.Start(lamp, null);

        machine.Fire(lamp, Switch.Toggle, null).IsSuccess.ShouldBeTrue();

        lamp.State.ShouldBe(Light.On);
        lamp.StateTransitions[^1].ShouldBe(new StateTransition("State", "Off", "On", "Toggle", null, null));
    }

    private static readonly Error _invalid = Error.BusinessRule("Lamp.InvalidTransition", "不能这样切换");

    private static StateMachineBuilder<Lamp, Light, Switch> Define() => Lamp.DefineMachine();

    private enum Light
    {
        Off,
        On,
    }

    private enum Switch
    {
        Toggle,
    }

    // 状态属性的 setter 必须私有（KM0401），声明状态机的代码因此放在类型内部
    private sealed class Lamp() : AggregateRoot<Guid>(Guid.CreateVersion7())
    {
        public Light State { get; private set; }

        public string Text { get; private set; } = null!;

        public static StateMachineBuilder<Lamp, Light, Switch> DefineMachine() =>
            StateMachine.Define<Lamp, Light, Switch>(l => l.State, (l, v) => l.State = v);

        public static void DefineByText() => StateMachine.Define<Lamp, string, Switch>(l => l.Text, (l, v) => l.Text = v);
    }
}
