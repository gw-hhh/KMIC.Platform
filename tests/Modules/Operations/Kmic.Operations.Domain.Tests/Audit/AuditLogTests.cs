using System.Net;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Operations.Domain.Audit;
using Shouldly;

namespace Kmic.Operations.Domain.Tests.Audit;

public sealed class AuditLogTests
{
    private static readonly DateTimeOffset _at = new(2026, 11, 13, 8, 30, 0, TimeSpan.Zero);

    private static AuditLog Minimal() =>
        AuditLog.Record(AuditLogId.From(Guid.CreateVersion7()), _at, "Identity", "Identity.CreateUser", "Http", "0123456789abcdef0123456789abcdef", "[]", "[]", "{}");

    [Fact]
    public void Record_KeepsEverythingItWasGiven()
    {
        var id = AuditLogId.From(Guid.CreateVersion7());
        var actor = UserId.New();
        var role = RoleId.New();
        var session = SessionId.New();

        var entry = AuditLog.Record(
            id,
            _at,
            "Identity",
            "Identity.CreateUser",
            "Http",
            "0123456789abcdef0123456789abcdef",
            """[{"resourceType":"User","resourceId":"6f2a0f6e-0f64-4d8e-9a53-3a2b45c0b7aa"}]""",
            """[{"entity":"User"}]""",
            """{"rows":"3"}""",
            actorUserId: actor,
            roleId: role,
            sessionId: session,
            clientIp: IPAddress.Parse("10.1.2.3"));

        entry.Id.ShouldBe(id);
        entry.OccurredAt.ShouldBe(_at);
        entry.Module.ShouldBe("Identity");
        entry.Action.ShouldBe("Identity.CreateUser");
        entry.Origin.ShouldBe("Http");
        entry.ActorUserId.ShouldBe(actor);
        entry.RoleId.ShouldBe(role);
        entry.SessionId.ShouldBe(session);
        entry.ClientIp.ShouldBe(IPAddress.Parse("10.1.2.3"));
        entry.Targets.ShouldContain("resourceType");
        entry.Details.ShouldBe("""{"rows":"3"}""");
    }

    [Fact]
    public void Record_WithoutTheOptionalParts_LeavesThemEmpty()
    {
        var entry = Minimal();

        entry.ActorUserId.ShouldBeNull();
        entry.ActualUserId.ShouldBeNull();
        entry.RoleId.ShouldBeNull();
        entry.SessionId.ShouldBeNull();
        entry.OnBehalfOfUserId.ShouldBeNull();
        entry.ClientIp.ShouldBeNull();
    }

    [Theory]
    [InlineData("", "Identity.CreateUser", "Http")]
    [InlineData("Identity", " ", "Http")]
    [InlineData("Identity", "Identity.CreateUser", "")]
    public void Record_RefusesABlankModuleActionOrOrigin(string module, string action, string origin) =>
        Should.Throw<ArgumentException>(() => AuditLog.Record(AuditLogId.From(Guid.CreateVersion7()), _at, module, action, origin, "trace", "[]", "[]", "{}"));

    [Fact]
    public void AnEmptyId_IsNotAnId() =>
        Should.Throw<Vogen.ValueObjectValidationException>(() => AuditLogId.From(Guid.Empty));
}
