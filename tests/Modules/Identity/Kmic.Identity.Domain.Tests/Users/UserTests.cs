using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Kmic.Identity.Domain.Users.Events;
using Shouldly;

namespace Kmic.Identity.Domain.Tests.Users;

public sealed class UserTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private static User Newcomer() =>
        User.Create("13800138000", "张三", null, "E001", "138 0013 8000", "zhang@example.com").ShouldBeSuccess();

    [Fact]
    public void Create_NormalizesFields_AndStartsActive()
    {
        var user = User.Create("  13800138000 ", " 张三 ", "", " E001 ", "13800138000", " zhang@example.com ").ShouldBeSuccess();

        user.Account.ShouldBe("13800138000");
        user.DisplayName.ShouldBe("张三");
        user.EnglishName.ShouldBeNull();
        user.EmployeeNo.ShouldBe("E001");
        user.Mobile.ShouldBe("+8613800138000");
        user.Email.ShouldBe("zhang@example.com");
        user.Status.ShouldBe(UserStatus.Active);
        user.Kind.ShouldBe(UserKind.Normal);
        user.CanLogin.ShouldBeTrue();
        user.PasswordHash.ShouldBeNull();
        user.StateTransitions.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(null, "张三")]
    [InlineData("", "张三")]
    [InlineData("a b", "张三")]
    [InlineData("13800138000", "")]
    public void Create_InvalidAccountOrName_IsRefused(string? account, string? name) =>
        User.Create(account, name, null, null, null, null).IsFailure.ShouldBeTrue();

    [Fact]
    public void Create_InvalidOptionalFields_ReportTheirOwnError()
    {
        User.Create("a", "b", null, null, "123", null).ShouldBeFailure(IdentityErrors.MobileInvalid);
        User.Create("a", "b", null, null, null, "not-an-email").ShouldBeFailure(IdentityErrors.EmailInvalid);
        User.Create("a", "b", null, new string('9', 51), null, null).ShouldBeFailure(IdentityErrors.EmployeeNoInvalid);
    }

    [Fact]
    public void UpdateProfile_ReplacesAllFields()
    {
        var user = Newcomer();

        user.UpdateProfile("zhang", "张三丰", "Zhang", null, null, null).ShouldBeSuccess();

        user.Account.ShouldBe("zhang");
        user.DisplayName.ShouldBe("张三丰");
        user.EnglishName.ShouldBe("Zhang");
        user.EmployeeNo.ShouldBeNull();
        user.Mobile.ShouldBeNull();
    }

    [Fact]
    public void Disable_FromActive_PublishesTheEvent_AndStopsLogin()
    {
        var user = Newcomer();

        user.Disable("离职").ShouldBeSuccess();

        user.Status.ShouldBe(UserStatus.Disabled);
        user.CanLogin.ShouldBeFalse();
        user.DomainEvents.ShouldHaveSingleItem().ShouldBe(new UserDisabled(user.Id));
        user.Disable().ShouldBeFailure(IdentityErrors.InvalidState);
    }

    [Fact]
    public void Enable_FromDisabled_BringsLoginBack()
    {
        var user = Newcomer();
        user.Disable();

        user.Enable().ShouldBeSuccess();

        user.CanLogin.ShouldBeTrue();
        user.Enable().ShouldBeFailure(IdentityErrors.InvalidState);
    }

    [Fact]
    public void SetPassword_RecordsTheChange_AndRaisesTheEvent()
    {
        var user = Newcomer();

        user.SetPassword("hash-1", mustChange: true, byAdministrator: true, _now).ShouldBeSuccess();

        user.PasswordHash.ShouldBe("hash-1");
        user.PasswordChangeRequired.ShouldBeTrue();
        user.PasswordChangedAt.ShouldBe(_now);
        user.DomainEvents.ShouldHaveSingleItem().ShouldBe(new PasswordChanged(user.Id, ByAdministrator: true));
    }

    [Fact]
    public void RehashPassword_ReplacesTheHashWithoutAChangeEvent()
    {
        var user = Newcomer();
        user.SetPassword("hash-1", false, false, _now);
        user.ClearPendingRecords();

        user.RehashPassword("hash-2");

        user.PasswordHash.ShouldBe("hash-2");
        user.PasswordChangedAt.ShouldBe(_now);
        user.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Grant_AddsOnce_WithItsAnchors()
    {
        var user = Newcomer();
        var role = RoleId.New();
        var east = OrgUnitId.New();

        var grant = user.Grant(role, UserId.New(), _now, [east, east]).ShouldBeSuccess();

        grant.RoleId.ShouldBe(role);
        grant.Anchors.ShouldHaveSingleItem().OrgUnitId.ShouldBe(east);
        user.HasRole(role).ShouldBeTrue();
        user.Grant(role, UserId.New(), _now, []).ShouldBeFailure(IdentityErrors.GrantExists);
    }

    [Fact]
    public void SetAnchors_ReplacesTheSet_KeepingUnchangedOnes()
    {
        var user = Newcomer();
        var role = RoleId.New();
        var a = OrgUnitId.New();
        var b = OrgUnitId.New();
        var c = OrgUnitId.New();
        var grant = user.Grant(role, UserId.New(), _now, [a, b]).ShouldBeSuccess();
        var keptAnchorId = grant.Anchors.Single(x => x.OrgUnitId == a).Id;

        user.SetAnchors(role, [a, c]).ShouldBeSuccess();

        grant.Anchors.Select(x => x.OrgUnitId).ShouldBe([a, c], ignoreOrder: true);
        grant.Anchors.Single(x => x.OrgUnitId == a).Id.ShouldBe(keptAnchorId);
        user.SetAnchors(RoleId.New(), []).ShouldBeFailure(IdentityErrors.GrantNotFound);
    }

    [Fact]
    public void Revoke_RemovesTheGrant_AndForgetsItAsLastRole()
    {
        var user = Newcomer();
        var role = RoleId.New();
        user.Grant(role, UserId.New(), _now, []);
        user.RecordRoleUsed(role);

        user.Revoke(role).ShouldBeSuccess();

        user.HasRole(role).ShouldBeFalse();
        user.LastRoleId.ShouldBeNull();
        user.Revoke(role).ShouldBeFailure(IdentityErrors.GrantNotFound);
    }

    [Fact]
    public void AssignOrgUnit_SetsAndClears()
    {
        var user = Newcomer();
        var unit = OrgUnitId.New();

        user.AssignOrgUnit(unit).ShouldBeSuccess();
        user.OrgUnitId.ShouldBe(unit);
        user.AssignOrgUnit(null).ShouldBeSuccess();
        user.OrgUnitId.ShouldBeNull();
    }
}
