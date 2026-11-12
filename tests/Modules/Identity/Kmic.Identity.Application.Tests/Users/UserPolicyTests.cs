using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Identity.Application.Users;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Users;

public sealed class UserPolicyTests
{
    private static readonly UserAdminAction[] _credentialActions = [UserAdminAction.ResetPassword, UserAdminAction.ChangeCredentials, UserAdminAction.ForceLogout];

    private static readonly UserAdminAction[] _impersonation = [UserAdminAction.Impersonate, UserAdminAction.ImpersonateWrite];

    private readonly UserPolicy _policy = new();

    private static User Target() => User.Create("13800138000", "张三", null, null, null, null).Value;

    private static TestIdentity Operator() => TestIdentity.User("管理员");

    private PolicyDecision Evaluate(TestIdentity access, UserTarget target, UserAdminAction action) => _policy.Evaluate(access, target, action);

    [Fact]
    public void AnOrdinaryUser_CanBeManagedInEveryWay()
    {
        var target = new UserTarget(Target(), OperatorIsProtected: false, HoldsProtectedRole: false, IsLastProtectedHolder: false);

        foreach (var action in UserAdminAction.All.Except(_impersonation))
        {
            Evaluate(Operator(), target, action).Allowed.ShouldBeTrue(action.Code);
        }
    }

    [Fact]
    public void Yourself_CannotBeDisabledOrForcedOffline_ButCanHaveOtherThingsDone()
    {
        var user = Target();
        var access = Operator() with { UserId = user.Id };
        var target = new UserTarget(user, OperatorIsProtected: false, HoldsProtectedRole: false, IsLastProtectedHolder: false);

        Evaluate(access, target, UserAdminAction.Disable).Error.ShouldBe(IdentityErrors.CannotActOnSelf);
        Evaluate(access, target, UserAdminAction.ForceLogout).Error.ShouldBe(IdentityErrors.CannotActOnSelf);
        Evaluate(access, target, UserAdminAction.Edit).Allowed.ShouldBeTrue();
        Evaluate(access, target, UserAdminAction.ResetPassword).Allowed.ShouldBeTrue();
    }

    [Fact]
    public void ACredentialAction_OnAProtectedHolder_NeedsAProtectedRole()
    {
        var target = new UserTarget(Target(), OperatorIsProtected: false, HoldsProtectedRole: true, IsLastProtectedHolder: false);

        foreach (var action in UserAdminAction.All.Except(_impersonation))
        {
            var decision = Evaluate(Operator(), target, action);
            if (_credentialActions.Contains(action))
            {
                decision.Allowed.ShouldBeFalse(action.Code);
                decision.Error.ShouldBe(IdentityErrors.ProtectedRoleChangeDenied);
            }
            else
            {
                decision.Allowed.ShouldBeTrue(action.Code);
            }
        }
    }

    [Fact]
    public void ACredentialAction_OnAProtectedHolder_IsAllowedForAProtectedOperator()
    {
        var target = new UserTarget(Target(), OperatorIsProtected: true, HoldsProtectedRole: true, IsLastProtectedHolder: false);

        foreach (var action in UserAdminAction.All.Except(_impersonation))
        {
            Evaluate(Operator(), target, action).Allowed.ShouldBeTrue(action.Code);
        }
    }

    [Fact]
    public void TheLastProtectedHolder_CannotBeDisabled_EvenByAProtectedOperator()
    {
        var target = new UserTarget(Target(), OperatorIsProtected: true, HoldsProtectedRole: true, IsLastProtectedHolder: true);

        Evaluate(Operator(), target, UserAdminAction.Disable).Error.ShouldBe(IdentityErrors.LastProtectedHolder);
        Evaluate(Operator(), target, UserAdminAction.Edit).Allowed.ShouldBeTrue();
        Evaluate(Operator(), target, UserAdminAction.ResetPassword).Allowed.ShouldBeTrue();
    }

    [Fact]
    public void Impersonating_IsRefusedForYourselfAndProtectedHolders_EvenByAProtectedOperator()
    {
        var user = Target();
        var self = Operator() with { UserId = user.Id };
        var ordinary = new UserTarget(user, OperatorIsProtected: false, HoldsProtectedRole: false, IsLastProtectedHolder: false);
        var protectedHolder = new UserTarget(Target(), OperatorIsProtected: true, HoldsProtectedRole: true, IsLastProtectedHolder: false);

        foreach (var action in _impersonation)
        {
            Evaluate(Operator(), ordinary, action).Allowed.ShouldBeTrue(action.Code);
            Evaluate(self, ordinary, action).Error.ShouldBe(IdentityErrors.ImpersonationDenied);
            Evaluate(Operator(), protectedHolder, action).Error.ShouldBe(IdentityErrors.ImpersonationDenied);
        }
    }
}
