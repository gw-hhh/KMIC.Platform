using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Identity.Application.Users.Grants;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Users;

public sealed class GrantPolicyTests
{
    private readonly GrantPolicy _policy = new();

    private static User Target() => User.Create("13800138000", "张三", null, null, null, null).Value;

    private static GrantTarget For(User user, bool roleProtected = false, bool operatorProtected = false, bool removesLast = false) =>
        new(user, RoleId.New(), roleProtected, RoleIsActive: true, operatorProtected, removesLast);

    [Fact]
    public void AnOrdinaryRole_CanBeGrantedRevokedAndAnchoredByAnyone()
    {
        var target = For(Target());

        foreach (var action in GrantAction.All)
        {
            _policy.Evaluate(TestIdentity.User("管理员"), target, action).Allowed.ShouldBeTrue(action.Code);
        }
    }

    [Fact]
    public void NobodyCanChangeTheirOwnGrants_EvenWithAProtectedRole()
    {
        var user = Target();
        var access = TestIdentity.User("自己") with { UserId = user.Id };
        var target = For(user, roleProtected: true, operatorProtected: true);

        foreach (var action in GrantAction.All)
        {
            _policy.Evaluate(access, target, action).Error.ShouldBe(IdentityErrors.CannotChangeOwnGrants);
        }
    }

    [Fact]
    public void AProtectedRole_NeedsAProtectedOperator_ForEveryAction()
    {
        foreach (var action in GrantAction.All)
        {
            _policy.Evaluate(TestIdentity.User("管理员"), For(Target(), roleProtected: true), action).Error.ShouldBe(IdentityErrors.ProtectedRoleChangeDenied);
            _policy.Evaluate(TestIdentity.User("管理员"), For(Target(), roleProtected: true, operatorProtected: true), action).Allowed.ShouldBeTrue(action.Code);
        }
    }

    [Fact]
    public void RevokingTheLastProtectedHolder_IsRefused_ButGrantingIsNot()
    {
        var target = For(Target(), roleProtected: true, operatorProtected: true, removesLast: true);

        _policy.Evaluate(TestIdentity.User("管理员"), target, GrantAction.Revoke).Error.ShouldBe(IdentityErrors.LastProtectedHolder);
        _policy.Evaluate(TestIdentity.User("管理员"), target, GrantAction.Grant).Allowed.ShouldBeTrue();
        _policy.Evaluate(TestIdentity.User("管理员"), target, GrantAction.SetAnchors).Allowed.ShouldBeTrue();
    }
}
