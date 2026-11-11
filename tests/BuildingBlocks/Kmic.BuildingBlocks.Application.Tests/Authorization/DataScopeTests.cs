using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Testing.Identity;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Authorization;

public sealed class DataScopeTests
{
    private const string Orders = "Sales.Order";
    private const string Customers = "Sales.Customer";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq
        .Unit("华东", east => east.Unit("上海").Unit("杭州"))
        .Unit("华南", south => south.Unit("广州"))));

    private static readonly TestIdentity _shanghaiSales = TestIdentity.User("销售甲").InTree(_org).InOrgUnit(_org["上海"]);

    [Fact]
    public void For_SystemIdentity_Unrestricted()
    {
        var scope = ScopeOf(CallOrigin.Inbox, CurrentUser.System(), access: null);

        scope.Unrestricted.ShouldBeTrue();
        scope.For(Orders).All.ShouldBeTrue();
    }

    [Fact]
    public void For_Anonymous_SeesNothing()
    {
        var scope = ScopeOf(CallOrigin.Http, CurrentUser.Anonymous, access: null);

        scope.Unrestricted.ShouldBeFalse();
        var orders = scope.For(Orders);
        orders.All.ShouldBeFalse();
        orders.Owner.ShouldBeNull();
        orders.OrgUnits.ShouldBeEmpty();
    }

    [Fact]
    public void For_Self_OnlyOwner()
    {
        var actor = _shanghaiSales.WithRole(r => r.Scope(ScopeLevel.Self));

        var scope = ScopeOf(actor).For(Orders);

        scope.All.ShouldBeFalse();
        scope.Owner.ShouldBe(actor.UserId);
        scope.OrgUnits.ShouldBeEmpty();
    }

    [Fact]
    public void For_OrgUnit_OwnOrgUnitOnly()
    {
        var actor = _shanghaiSales.WithRole(r => r.Scope(ScopeLevel.OrgUnit));

        ScopeOf(actor).For(Orders).OrgUnits.ShouldBe([_org["上海"]]);
    }

    [Fact]
    public void For_OrgUnitAndBelowWithAnchors_AnchorsReplaceOwnOrgUnit()
    {
        var actor = TestIdentity.User("经理").InTree(_org).InOrgUnit(_org["总部"])
            .WithRole(r => r.Scope(ScopeLevel.OrgUnitAndBelow))
            .WithAnchors(_org["华东"], _org["华南"]);

        var scope = ScopeOf(actor).For(Orders);

        scope.Owner.ShouldBe(actor.UserId);
        scope.OrgUnits.ShouldBe([_org["华东"], _org["上海"], _org["杭州"], _org["华南"], _org["广州"]], ignoreOrder: true);
    }

    [Fact]
    public void For_OverrideForOneScopeObject_OthersUseDefault()
    {
        var actor = _shanghaiSales.WithRole(r => r.Scope(ScopeLevel.OrgUnit).Override(Customers, ScopeLevel.All));

        var scope = ScopeOf(actor);

        scope.For(Customers).All.ShouldBeTrue();
        scope.For(Orders).All.ShouldBeFalse();
        scope.For(Orders).OrgUnits.ShouldBe([_org["上海"]]);
    }

    [Fact]
    public void For_CalledRepeatedly_ComputedOnce()
    {
        // 每执行一次查询，EF 要读 All、Owner、OrgUnits 三次
        var scope = ScopeOf(_shanghaiSales.WithRole(r => r.Scope(ScopeLevel.OrgUnitAndBelow)));

        scope.For(Orders).ShouldBeSameAs(scope.For(Orders));
    }

    private static DataScope ScopeOf(TestIdentity identity) => ScopeOf(CallOrigin.Http, CurrentUser.From(identity), identity);

    private static DataScope ScopeOf(CallOrigin origin, ICurrentUser user, IAccessContext? access)
    {
        var execution = new ExecutionScope();
        execution.Initialize(origin, user, access);
        return new DataScope(execution);
    }
}
