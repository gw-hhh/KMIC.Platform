using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Operations.Application;
using Kmic.Operations.Application.Audit.Record;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 审计汇总与查询：落库、幂等、筛选、游标翻页、详情，端到端
/// </summary>
public sealed class AuditTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations";

    private static readonly string[] _view = [OperationsPermissions.AuditView];

    private readonly DateTimeOffset _now = TimeProvider.System.GetUtcNow();

    private static AuditRecorded Message(Guid id, DateTimeOffset at, string module, string action, Guid? actor = null, Guid? target = null, string targetType = "Specimen") =>
        new()
        {
            AuditId = id,
            OccurredAt = at,
            Module = module,
            Action = action,
            Origin = "Http",
            ActorUserId = actor,
            ClientIp = "10.1.2.3",
            TraceId = "0123456789abcdef0123456789abcdef",
            Targets = target is { } resource ? [new AuditTarget(targetType, resource)] : [],
            Changes = target is null ? [] : [new EntityChange(targetType, target.Value, ChangeKind.Modified, [new FieldChange("Title", "旧标题", "新标题", Masked: false), new FieldChange("Secret", null, null, Masked: true)])],
            Details = new Dictionary<string, string>(StringComparer.Ordinal) { ["rows"] = "3" },
        };

    private async Task RecordAsync(AuditRecorded message)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        (await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new RecordAuditCommand(message), Ct)).ShouldBeSuccess();
    }

    private Task<HttpResponseMessage> ListAsync(string token, string query) =>
        SendAsync(HttpMethod.Get, $"{Root}/audit?{query}", token);

    private string Range(int fromDaysAgo = 2, int toDaysAhead = 1) =>
        $"from={Uri.EscapeDataString(_now.AddDays(-fromDaysAgo).ToString("O"))}&to={Uri.EscapeDataString(_now.AddDays(toDaysAhead).ToString("O"))}";

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task RecordingTheSameAuditTwice_WritesItOnce()
    {
        var id = Guid.CreateVersion7();

        await RecordAsync(Message(id, _now.AddMinutes(-5), "Sample", "Sample.CreateSpecimen"));
        await RecordAsync(Message(id, _now.AddMinutes(-5), "Sample", "Sample.CreateSpecimen"));

        (await Seed.CountAsync($"""SELECT count(*) FROM "Operations"."AuditLog" WHERE "Id" = '{id}'""")).ShouldBe(1);
        (await Seed.ScalarAsync<string>($"""SELECT host("ClientIp") FROM "Operations"."AuditLog" WHERE "Id" = '{id}'""")).ShouldBe("10.1.2.3");
    }

    [Fact]
    public async Task TheAuditTable_CannotBeRewrittenByTheRuntimeAccount()
    {
        var id = Guid.CreateVersion7();
        await RecordAsync(Message(id, _now.AddMinutes(-5), "Sample", "Sample.CreateSpecimen"));

        var update = await Should.ThrowAsync<Npgsql.PostgresException>(async () => await Seed.SqlAsync($"""UPDATE "Operations"."AuditLog" SET "Action" = 'x' WHERE "Id" = '{id}'"""));
        var delete = await Should.ThrowAsync<Npgsql.PostgresException>(async () => await Seed.SqlAsync($"""DELETE FROM "Operations"."AuditLog" WHERE "Id" = '{id}'"""));

        update.SqlState.ShouldBe("42501");
        delete.SqlState.ShouldBe("42501");
    }

    [Fact]
    public async Task TheList_NeedsThePermission_AndARangeOfAtMostNinetyThreeDays()
    {
        var plain = await LoginAsOperatorAsync("audit-plain", []);
        var viewer = await LoginAsOperatorAsync("audit-range-viewer", _view);

        var forbidden = await ListAsync(plain, Range());
        var noRange = await ListAsync(viewer, "limit=10");
        var tooLong = await ListAsync(viewer, $"from={Uri.EscapeDataString(_now.AddDays(-100).ToString("O"))}&to={Uri.EscapeDataString(_now.ToString("O"))}");
        var backwards = await ListAsync(viewer, Range(fromDaysAgo: -2, toDaysAhead: -3));

        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        noRange.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(tooLong)).GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("path").GetString()).ShouldBe(["to"]);
        backwards.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TheList_FiltersByActorModuleActionAndTarget_NewestFirst()
    {
        var viewer = await LoginAsOperatorAsync("audit-filter-viewer", _view);
        var actor = Guid.CreateVersion7();
        var other = Guid.CreateVersion7();
        var resource = Guid.CreateVersion7();
        var marker = $"Test.Filter{Guid.CreateVersion7():N}";
        await RecordAsync(Message(Guid.CreateVersion7(), _now.AddMinutes(-30), "Sample", marker, actor, resource));
        await RecordAsync(Message(Guid.CreateVersion7(), _now.AddMinutes(-20), "Sample", marker, other, Guid.CreateVersion7()));
        await RecordAsync(Message(Guid.CreateVersion7(), _now.AddMinutes(-10), "Identity", marker, actor, Guid.CreateVersion7(), "User"));

        var all = await ItemsAsync(viewer, $"{Range()}&action={marker}");
        var byActor = await ItemsAsync(viewer, $"{Range()}&action={marker}&actorUserId={actor}");
        var byModule = await ItemsAsync(viewer, $"{Range()}&action={marker}&module=Sample");
        var byResource = await ItemsAsync(viewer, $"{Range()}&action={marker}&resourceId={resource}");
        var byType = await ItemsAsync(viewer, $"{Range()}&action={marker}&resourceType=User");
        var byTypeAndId = await ItemsAsync(viewer, $"{Range()}&action={marker}&resourceType=Specimen&resourceId={resource}");
        var outsideRange = await ItemsAsync(viewer, $"{Range(fromDaysAgo: 10, toDaysAhead: -9)}&action={marker}");

        all.Select(i => i.GetProperty("occurredAt").GetDateTimeOffset()).ShouldBe(all.Select(i => i.GetProperty("occurredAt").GetDateTimeOffset()).OrderByDescending(t => t));
        all.Count.ShouldBe(3);
        byActor.Count.ShouldBe(2);
        byModule.Count.ShouldBe(2);
        byResource.ShouldHaveSingleItem().GetProperty("targets")[0].GetProperty("resourceId").GetGuid().ShouldBe(resource);
        byType.ShouldHaveSingleItem().GetProperty("module").GetString().ShouldBe("Identity");
        byTypeAndId.Count.ShouldBe(1);
        outsideRange.ShouldBeEmpty();
    }

    [Fact]
    public async Task WalkingThePagesWithTheCursor_VisitsEveryRowOnce()
    {
        var viewer = await LoginAsOperatorAsync("audit-cursor-viewer", _view);
        var marker = $"Test.Cursor{Guid.CreateVersion7():N}";
        for (var i = 0; i < 7; i++)
        {
            await RecordAsync(Message(Guid.CreateVersion7(), _now.AddMinutes(-60 + i), "Sample", marker));
        }

        var seen = new List<Guid>();
        string? after = null;
        var pages = 0;
        do
        {
            var response = await ListAsync(viewer, $"{Range()}&action={marker}&limit=3" + (after is null ? string.Empty : $"&after={Uri.EscapeDataString(after)}"));
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var page = await JsonAsync(response);
            seen.AddRange(page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()));
            after = page.GetProperty("nextCursor").ValueKind == JsonValueKind.Null ? null : page.GetProperty("nextCursor").GetString();
            pages++;
        }
        while (after is not null);

        seen.Count.ShouldBe(7);
        seen.Distinct().Count().ShouldBe(7);
        pages.ShouldBe(3);
        var tampered = await ListAsync(viewer, $"{Range()}&action={marker}&limit=3&after=nonsense");
        tampered.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(tampered)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("after");
    }

    [Fact]
    public async Task TheDetail_ShowsTheFieldChangesAndTheResolvedNames_AndAnUnknownIdIsNotFound()
    {
        var viewer = await LoginAsOperatorAsync("audit-detail-viewer", _view);
        var plain = await LoginAsOperatorAsync("audit-detail-plain", []);
        var actorRole = await Seed.RoleAsync("做操作的角色", []);
        var actor = await Seed.UserAsync("audit-detail-actor", Password, displayName: "做操作的人");
        var id = Guid.CreateVersion7();
        var message = Message(id, _now.AddMinutes(-5), "Sample", "Sample.UpdateSpecimen", actor, Guid.CreateVersion7());
        await RecordAsync(message with { RoleId = actorRole });

        var response = await SendAsync(HttpMethod.Get, $"{Root}/audit/{id}", viewer);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await JsonAsync(response);
        detail.GetProperty("actorName").GetString().ShouldBe("做操作的人");
        detail.GetProperty("roleName").GetString().ShouldBe("做操作的角色");
        detail.GetProperty("clientIp").GetString().ShouldBe("10.1.2.3");
        detail.GetProperty("traceId").GetString().ShouldBe("0123456789abcdef0123456789abcdef");
        detail.GetProperty("details").GetProperty("rows").GetString().ShouldBe("3");
        var change = detail.GetProperty("changes")[0];
        change.GetProperty("entity").GetString().ShouldBe("Specimen");
        change.GetProperty("fields").EnumerateArray().Single(f => string.Equals(f.GetProperty("field").GetString(), "Title", StringComparison.Ordinal)).GetProperty("new").GetString().ShouldBe("新标题");
        change.GetProperty("fields").EnumerateArray().Single(f => f.GetProperty("masked").GetBoolean()).GetProperty("old").ValueKind.ShouldBe(JsonValueKind.Null);
        (await SendAsync(HttpMethod.Get, $"{Root}/audit/{id}", plain)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var unknown = await SendAsync(HttpMethod.Get, $"{Root}/audit/{Guid.CreateVersion7()}", viewer);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Operations.AuditNotFound");
    }

    private async Task<List<JsonElement>> ItemsAsync(string token, string query)
    {
        var response = await ListAsync(token, query);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return [.. (await JsonAsync(response)).GetProperty("items").EnumerateArray()];
    }
}
