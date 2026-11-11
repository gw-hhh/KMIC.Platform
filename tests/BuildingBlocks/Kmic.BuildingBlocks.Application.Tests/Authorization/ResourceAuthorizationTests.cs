using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Security;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Authorization;

/// <summary>
/// 资源策略（第 3 层）与 allowedActions：同一份策略既用于处理器检查，也用于计算按钮
/// </summary>
public sealed class ResourceAuthorizationTests
{
    private static readonly Error _notOwner = Error.Forbidden("Thing.NotOwner", "只有负责人可以操作");
    private static readonly Error _notDraft = Error.BusinessRule("Thing.NotDraft", "只有草稿可以修改");

    private readonly RecordingSecurityEventWriter _securityEvents = new();

    [Fact]
    public void Authorizer_Owner_Allowed_OtherDeniedWithSecurityEvent()
    {
        var owner = TestAccess.WithPermissions();
        var thing = new Thing(owner.UserId, IsDraft: true);

        Authorizer(owner).Check(thing, ThingAction.Edit).IsSuccess.ShouldBeTrue();
        Authorizer(TestAccess.WithPermissions()).Check(thing, ThingAction.Edit).Error.ShouldBeSameAs(_notOwner);

        var denied = _securityEvents.Events.ShouldHaveSingleItem();
        denied.Reason.ShouldBe(nameof(DenialReason.ResourceDenied));
        denied.ResourceType.ShouldBe(nameof(Thing));
    }

    [Fact]
    public void Authorizer_SystemIdentity_Allowed()
    {
        var scope = new ExecutionScope();
        scope.Initialize(CallOrigin.Inbox, CurrentUser.System());
        var authorizer = new ResourceAuthorizer<Thing, ThingAction>(new ThingPolicy(), scope, _securityEvents);

        authorizer.Check(new Thing(UserId.New(), IsDraft: true), ThingAction.Edit).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void AllowedActions_RequiresPermissionPolicyAndState()
    {
        var owner = TestAccess.WithPermissions(ThingActions.EditPermission, ThingActions.ViewPermission);
        var draft = new Thing(owner.UserId, IsDraft: true);
        var submitted = draft with { IsDraft = false };
        var someoneElses = new Thing(UserId.New(), IsDraft: true);

        AllowedFor(owner, draft).ShouldBe(["Edit", "Print"]);
        AllowedFor(owner, submitted).ShouldBe(["Print"]);
        AllowedFor(owner, someoneElses).ShouldBe(["Print"]);
        AllowedFor(TestAccess.WithPermissions(ThingActions.ViewPermission), draft).ShouldBe(["Print"]);
    }

    [Fact]
    public void AllowedActions_ReadOnlyImpersonation_KeepsReadOnlyActions()
    {
        var owner = TestAccess.WithPermissions(ThingActions.EditPermission, ThingActions.ViewPermission) with
        {
            ImpersonatorUserId = UserId.New(),
            ImpersonationAllowsWrite = false,
        };

        AllowedFor(owner, new Thing(owner.UserId, IsDraft: true)).ShouldBe(["Print"]);
    }

    private static IReadOnlyList<string> AllowedFor(IAccessContext access, Thing thing) =>
        new ThingActions().Evaluate(access, new ThingPolicy(), thing);

    private ResourceAuthorizer<Thing, ThingAction> Authorizer(IAccessContext access)
    {
        var scope = new ExecutionScope();
        scope.Initialize(CallOrigin.Http, CurrentUser.From(access), access);
        return new ResourceAuthorizer<Thing, ThingAction>(new ThingPolicy(), scope, _securityEvents);
    }

    public enum ThingAction
    {
        Edit,
        Print,
    }

    public sealed record Thing(UserId OwnerId, bool IsDraft);

    public sealed class ThingPolicy : IResourcePolicy<Thing, ThingAction>
    {
        public PolicyDecision Evaluate(IAccessContext access, Thing resource, ThingAction action) => action switch
        {
            ThingAction.Edit => resource.OwnerId == access.UserId ? PolicyDecision.Allow : PolicyDecision.Deny(DenialReason.ResourceDenied, _notOwner),
            ThingAction.Print => PolicyDecision.Allow,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
    }

    public sealed class ThingActions : ResourceActionCatalog<Thing, ThingAction>
    {
        public const string EditPermission = "Thing.Thing.Edit";
        public const string ViewPermission = "Thing.Thing.View";

        protected override void Define(IResourceActionBuilder<Thing, ThingAction> builder)
        {
            builder.Action(ThingAction.Edit, EditPermission, state: t => t.IsDraft ? Result.Success() : _notDraft);
            builder.Action(ThingAction.Print, ViewPermission, readOnly: true);
        }
    }
}
