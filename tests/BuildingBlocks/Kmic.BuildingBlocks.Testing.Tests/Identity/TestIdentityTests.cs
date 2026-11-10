using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Identity;

public sealed class TestIdentityTests
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("华东", east => east.Unit("上海")).Unit("总部"));

    [Fact]
    public void WithRole_SetsPermissionsScopeAndOverrides()
    {
        var plain = TestIdentity.User("销售甲");

        var actor = plain
            .InTree(_org)
            .InOrgUnit(_org["上海"])
            .WithRole(r => r.Permissions("Thing.Thing.View", "Thing.Thing.Edit").Scope(ScopeLevel.OrgUnitAndBelow).Override("Thing.Thing", ScopeLevel.Self))
            .WithAnchors(_org["华东"]);

        actor.UserId.ShouldBe(plain.UserId);
        actor.RoleId.ShouldNotBe(plain.RoleId);
        actor.AuthorizationVersion.ShouldBe(plain.AuthorizationVersion + 1);
        actor.Permissions.ShouldBe(["Thing.Thing.View", "Thing.Thing.Edit"], ignoreOrder: true);
        actor.OrgUnitId.ShouldBe(_org["上海"]);
        actor.DataScopeGrant.DefaultLevel.ShouldBe(ScopeLevel.OrgUnitAndBelow);
        actor.DataScopeGrant.Overrides["Thing.Thing"].ShouldBe(ScopeLevel.Self);
        actor.DataScopeGrant.Anchors.ShouldBe([_org["华东"]]);
        plain.Permissions.ShouldBeEmpty();
    }

    [Fact]
    public void WithRole_KeepsAnchors() =>
        TestIdentity.User("经理乙").InTree(_org).WithAnchors(_org["华东"]).WithRole(r => r.Scope(ScopeLevel.OrgUnit))
            .DataScopeGrant.Anchors.ShouldBe([_org["华东"]]);

    [Fact]
    public void InOrgUnit_OutsideTree_Throws() =>
        Should.Throw<ArgumentException>(() => TestIdentity.User("销售甲").InOrgUnit(OrgUnitId.New()));

    [Fact]
    public async Task CreateScope_InitializesExecutionContext()
    {
        var impersonator = UserId.New();
        var actor = TestIdentity.User("销售甲").ImpersonatedBy(impersonator);
        await using var provider = new ServiceCollection().AddKmicApplication([]).BuildServiceProvider(validateScopes: true);
        var scopes = provider.GetRequiredService<IExecutionScopeFactory>();

        await using var userScope = scopes.CreateScope(actor);
        await using var systemScope = scopes.CreateSystemScope();

        var context = userScope.ServiceProvider.GetRequiredService<IExecutionContext>();
        context.Origin.ShouldBe(CallOrigin.Http);
        context.Access.ShouldBeSameAs(actor);
        context.User.UserId.ShouldBe(actor.UserId);
        context.User.ActualUserId.ShouldBe(impersonator);
        systemScope.ServiceProvider.GetRequiredService<IExecutionContext>().User.IsSystem.ShouldBeTrue();
    }
}
