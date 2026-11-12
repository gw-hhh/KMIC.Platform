using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Sessions;
using Shouldly;

namespace Kmic.Identity.Domain.Tests.Sessions;

public sealed class SessionTests
{
    private static readonly DateTimeOffset _start = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _refreshLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan _absoluteLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan _grace = TimeSpan.FromSeconds(10);

    private static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static Session NewSession() =>
        Session.Start(UserId.New(), RoleId.New(), ClientType.Web, "Chrome", "10.0.0.1", Hash("t0"), _start, _refreshLifetime, _absoluteLifetime);

    [Fact]
    public void Start_SetsExpiries_AndIssuesTheFirstToken()
    {
        var session = NewSession();

        session.Status.ShouldBe(SessionStatus.Active);
        session.RefreshExpiresAt.ShouldBe(_start + _refreshLifetime);
        session.AbsoluteExpiresAt.ShouldBe(_start + _absoluteLifetime);
        session.IsImpersonation.ShouldBeFalse();
        session.RefreshTokens.ShouldHaveSingleItem().IsCurrent.ShouldBeTrue();
        session.IsUsable(_start).ShouldBeTrue();
        session.IsUsable(_start + _absoluteLifetime).ShouldBeFalse();
    }

    [Fact]
    public void Start_RefreshLifetimeLongerThanAbsolute_IsCappedByTheAbsoluteLimit()
    {
        var session = Session.Start(UserId.New(), RoleId.New(), ClientType.App, null, null, Hash("t0"), _start, TimeSpan.FromDays(60), TimeSpan.FromDays(30));

        session.RefreshExpiresAt.ShouldBe(session.AbsoluteExpiresAt);
    }

    [Fact]
    public void Rotate_CurrentToken_IssuesANewOne_AndMarksTheOldAsUsed()
    {
        var session = NewSession();
        var now = _start.AddHours(1);

        var rotation = session.Rotate(Hash("t0"), Hash("t1"), now, _refreshLifetime, _grace);

        rotation.Outcome.ShouldBe(RotationOutcome.Rotated);
        rotation.NewToken.ShouldNotBeNull().IsCurrent.ShouldBeTrue();
        session.RefreshTokens.Count.ShouldBe(2);
        var old = session.RefreshTokens.Single(t => !t.IsCurrent);
        old.UsedAt.ShouldBe(now);
        old.ReplacedById.ShouldBe(rotation.NewToken!.Id);
        session.LastRefreshedAt.ShouldBe(now);
        session.RefreshExpiresAt.ShouldBe(now + _refreshLifetime);
    }

    [Fact]
    public void Rotate_NearTheAbsoluteLimit_NeverExtendsPastIt()
    {
        var session = NewSession();

        // 每 6 天刷新一次，一直刷到接近绝对上限
        for (var day = 6; day <= 24; day += 6)
        {
            session.Rotate(Hash($"t{(day / 6) - 1}"), Hash($"t{day / 6}"), _start.AddDays(day), _refreshLifetime, _grace).Outcome.ShouldBe(RotationOutcome.Rotated);
        }

        var last = session.Rotate(Hash("t4"), Hash("t5"), _start.AddDays(29), _refreshLifetime, _grace);

        last.Outcome.ShouldBe(RotationOutcome.Rotated);
        session.RefreshExpiresAt.ShouldBe(session.AbsoluteExpiresAt);
        last.NewToken!.ExpiresAt.ShouldBe(session.AbsoluteExpiresAt);
    }

    [Fact]
    public void Rotate_OldTokenWithinGrace_ContinuesFromTheCurrentToken()
    {
        var session = NewSession();
        session.Rotate(Hash("t0"), Hash("t1"), _start.AddHours(1), _refreshLifetime, _grace);

        var second = session.Rotate(Hash("t0"), Hash("t2"), _start.AddHours(1).AddSeconds(5), _refreshLifetime, _grace);

        second.Outcome.ShouldBe(RotationOutcome.Rotated);
        session.Status.ShouldBe(SessionStatus.Active);
        session.RefreshTokens.Count(t => t.IsCurrent).ShouldBe(1);
        session.RefreshTokens.Single(t => t.IsCurrent).Id.ShouldBe(second.NewToken!.Id);
    }

    [Fact]
    public void Rotate_OldTokenAfterGrace_IsReuse_AndRevokesTheSession()
    {
        var session = NewSession();
        session.Rotate(Hash("t0"), Hash("t1"), _start.AddHours(1), _refreshLifetime, _grace);
        var later = _start.AddHours(1).AddSeconds(11);

        var reuse = session.Rotate(Hash("t0"), Hash("t2"), later, _refreshLifetime, _grace);

        reuse.Outcome.ShouldBe(RotationOutcome.Reused);
        reuse.NewToken.ShouldBeNull();
        session.Status.ShouldBe(SessionStatus.Revoked);
        session.RevokedReason.ShouldBe(RevokeReason.RefreshTokenReused);
        session.RevokedAt.ShouldBe(later);
        session.RefreshTokens.Count.ShouldBe(2);
    }

    [Fact]
    public void Rotate_UnknownRevokedOrExpired_IsInvalid()
    {
        var session = NewSession();

        session.Rotate(Hash("nope"), Hash("t1"), _start, _refreshLifetime, _grace).Outcome.ShouldBe(RotationOutcome.Invalid);
        session.Rotate(Hash("t0"), Hash("t1"), _start + _refreshLifetime, _refreshLifetime, _grace).Outcome.ShouldBe(RotationOutcome.Invalid);

        session.Revoke(RevokeReason.Logout, _start);
        session.Rotate(Hash("t0"), Hash("t1"), _start, _refreshLifetime, _grace).Outcome.ShouldBe(RotationOutcome.Invalid);
    }

    [Fact]
    public void Revoke_KeepsTheFirstReason()
    {
        var session = NewSession();

        session.Revoke(RevokeReason.Logout, _start.AddMinutes(1));
        session.Revoke(RevokeReason.ForcedByAdmin, _start.AddMinutes(2));

        session.RevokedReason.ShouldBe(RevokeReason.Logout);
        session.RevokedAt.ShouldBe(_start.AddMinutes(1));
        session.IsUsable(_start.AddMinutes(3)).ShouldBeFalse();
    }

    [Fact]
    public void SwitchRole_ChangesTheCurrentRole()
    {
        var session = NewSession();
        var other = RoleId.New();

        session.SwitchRole(other);

        session.CurrentRoleId.ShouldBe(other);
    }

    [Fact]
    public void Impersonation_HasNoRefreshTokens_AndEndsAtItsLimit()
    {
        var impersonator = UserId.New();
        var session = Session.StartImpersonation(UserId.New(), RoleId.New(), impersonator, allowsWrite: false, " 排查权限问题 ", ClientType.Web, "Chrome", "10.0.0.1", _start, TimeSpan.FromHours(1));

        session.IsImpersonation.ShouldBeTrue();
        session.ImpersonatorUserId.ShouldBe(impersonator);
        session.ImpersonationAllowsWrite.ShouldBeFalse();
        session.ImpersonationReason.ShouldBe("排查权限问题");
        session.RefreshTokens.ShouldBeEmpty();
        session.AbsoluteExpiresAt.ShouldBe(_start.AddHours(1));
        session.IsUsable(_start.AddMinutes(59)).ShouldBeTrue();
        session.IsUsable(_start.AddHours(1)).ShouldBeFalse();
        session.Rotate(Hash("x"), Hash("y"), _start, _refreshLifetime, _grace).Outcome.ShouldBe(RotationOutcome.Invalid);
    }
}
