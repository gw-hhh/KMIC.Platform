using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Testing.Security;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Tests.Authorization.ResourceAuthorizationTests;

namespace Kmic.BuildingBlocks.Application.Tests.Authorization;

public sealed class ResourceAuthorizationRegistrationTests
{
    [Fact]
    public async Task Register_ResolvesAuthorizerAndAllowedActions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISecurityEventWriter>(new RecordingSecurityEventWriter());
        services.AddScoped<ExecutionScope>();
        services.AddScoped<IExecutionContext>(sp => sp.GetRequiredService<ExecutionScope>());

        var references = services.AddResourceAuthorization([typeof(ThingPolicy), typeof(ThingActions)]);

        references.ShouldBe(
        [
            new PermissionReference(ThingActions.EditPermission, "资源动作 ThingActions"),
            new PermissionReference(ThingActions.ViewPermission, "资源动作 ThingActions"),
        ]);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = provider.CreateAsyncScope();
        var access = TestAccess.WithPermissions(ThingActions.EditPermission);
        scope.ServiceProvider.GetRequiredService<ExecutionScope>().Initialize(CallOrigin.Http, CurrentUser.From(access), access);
        var thing = new Thing(access.UserId, IsDraft: true);

        scope.ServiceProvider.GetRequiredService<IAllowedActions<Thing>>().For(thing).ShouldBe(["Edit"]);
        scope.ServiceProvider.GetRequiredService<IResourceAuthorizer<Thing, ThingAction>>().Check(thing, ThingAction.Edit).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Register_WrongDeclarations_ReportsEachProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => new ServiceCollection().AddResourceAuthorization(
        [
            typeof(ThingPolicy),
            typeof(ThingActions),
            typeof(DoublePolicy),
            typeof(SecondThingActions),
            typeof(OrphanActions),
            typeof(ParameterizedActions),
            typeof(GadgetPolicy),
        ]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(DoublePolicy), StringComparison.Ordinal) && p.Contains("只能实现一个", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(DoublePolicy), StringComparison.Ordinal) && p.Contains("已有资源策略", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(SecondThingActions), StringComparison.Ordinal) && p.Contains("已有动作目录", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(OrphanActions), StringComparison.Ordinal) && p.Contains("没有 IResourcePolicy<Gizmo, ThingAction>", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(ParameterizedActions), StringComparison.Ordinal) && p.Contains("无参构造函数", StringComparison.Ordinal));
    }

    public sealed record Widget;

    public sealed record Gadget;

    public sealed record Gizmo;

    public sealed class DoublePolicy : IResourcePolicy<Thing, ThingAction>, IResourcePolicy<Widget, ThingAction>
    {
        public PolicyDecision Evaluate(IAccessContext access, Thing resource, ThingAction action) => PolicyDecision.Allow;

        public PolicyDecision Evaluate(IAccessContext access, Widget resource, ThingAction action) => PolicyDecision.Allow;
    }

    public sealed class GadgetPolicy : IResourcePolicy<Gadget, ThingAction>
    {
        public PolicyDecision Evaluate(IAccessContext access, Gadget resource, ThingAction action) => PolicyDecision.Allow;
    }

    public sealed class SecondThingActions : ResourceActionCatalog<Thing, ThingAction>
    {
        protected override void Define(IResourceActionBuilder<Thing, ThingAction> builder) =>
            builder.Action(ThingAction.Print, ThingActions.ViewPermission, readOnly: true);
    }

    public sealed class OrphanActions : ResourceActionCatalog<Gizmo, ThingAction>
    {
        protected override void Define(IResourceActionBuilder<Gizmo, ThingAction> builder) =>
            builder.Action(ThingAction.Print, ThingActions.ViewPermission, readOnly: true);
    }

    public sealed class ParameterizedActions(string permission) : ResourceActionCatalog<Gadget, ThingAction>
    {
        protected override void Define(IResourceActionBuilder<Gadget, ThingAction> builder) =>
            builder.Action(ThingAction.Print, permission, readOnly: true);
    }
}
