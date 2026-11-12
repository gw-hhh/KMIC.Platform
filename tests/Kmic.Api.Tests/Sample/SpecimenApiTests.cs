using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Sample;

/// <summary>
/// Sample 参考模块的端点：路由、状态码、错误格式、认证、权限、数据范围、并发版本、幂等
/// </summary>
public sealed class SpecimenApiTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimens";

    [Fact]
    public async Task Create_Returns201WithLocationIdAndVersion()
    {
        using var alice = ClientFor(Alice);

        var response = await PostCreateAsync(alice, Guid.CreateVersion7().ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await JsonAsync(response);
        var id = body.GetProperty("id").GetGuid();
        response.Headers.Location!.OriginalString.ShouldBe($"{Root}/{id}");
        body.GetProperty("version").GetInt64().ShouldBeGreaterThanOrEqualTo(0);
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["id", "version"]);
    }

    [Fact]
    public async Task Create_WithoutAnIdempotencyKey_Returns400()
    {
        using var alice = ClientFor(Alice);

        var response = await PostCreateAsync(alice, key: null);

        var body = await JsonAsync(response);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("Common.IdempotencyKeyRequired");
    }

    [Fact]
    public async Task Create_RetriedWithTheSameKey_ReplaysTheFirstResult()
    {
        using var alice = ClientFor(Alice);
        var key = Guid.CreateVersion7().ToString();

        var first = await PostCreateAsync(alice, key);
        var second = await PostCreateAsync(alice, key);

        second.StatusCode.ShouldBe(HttpStatusCode.Created);
        second.Headers.GetValues("Idempotent-Replayed").ShouldHaveSingleItem().ShouldBe("true");
        (await JsonAsync(second)).GetProperty("id").GetGuid().ShouldBe((await JsonAsync(first)).GetProperty("id").GetGuid());
        first.Headers.Contains("Idempotent-Replayed").ShouldBeFalse();
    }

    [Fact]
    public async Task Create_WithInvalidInput_Returns400WithFieldPaths()
    {
        using var alice = ClientFor(Alice);
        using var request = new HttpRequestMessage(HttpMethod.Post, Root)
        {
            Content = JsonContent.Create(new { title = "", items = new[] { new { name = "样品", quantity = 0 } } }),
        };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());

        var response = await alice.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await JsonAsync(response);
        body.GetProperty("code").GetString().ShouldBe("Common.ValidationFailed");
        body.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("path").GetString()).ShouldBe(["title", "items[0].quantity"], ignoreOrder: true);
    }

    [Fact]
    public async Task Create_WithoutThePermission_Returns403()
    {
        using var bob = ClientFor(Bob);

        var response = await PostCreateAsync(bob, Guid.CreateVersion7().ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.MissingPermission");
    }

    [Fact]
    public async Task NotLoggedIn_Returns401()
    {
        using var anonymous = ClientFor();

        var response = await anonymous.GetAsync(Root, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.Unauthenticated");
    }

    [Fact]
    public async Task Detail_HasVersionStatusAndAllowedActions()
    {
        using var alice = ClientFor(Alice);
        var (id, version) = await CreateAsync(alice, "详情测试");

        var response = await alice.GetAsync($"{Root}/{id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Contains("X-Authz-Version").ShouldBeTrue();
        var body = await JsonAsync(response);
        body.GetProperty("version").GetInt64().ShouldBe(version);
        body.GetProperty("status").GetString().ShouldBe("Draft");
        body.GetProperty("number").GetString().ShouldStartWith("SP-");
        body.GetProperty("items").GetArrayLength().ShouldBe(1);
        body.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()).ShouldBe(["Submit"]);
    }

    [Fact]
    public async Task Submit_ChangesTheStatusAndBumpsTheVersion()
    {
        using var alice = ClientFor(Alice);
        var (id, version) = await CreateAsync(alice, "提交测试");

        var submitted = await PostActionAsync(alice, $"{Root}/{id}/submit", new { expectedVersion = version });

        submitted.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newVersion = (await JsonAsync(submitted)).GetProperty("version").GetInt64();
        newVersion.ShouldBeGreaterThan(version);
        var detail = await JsonAsync(await alice.GetAsync($"{Root}/{id}", Ct));
        detail.GetProperty("status").GetString().ShouldBe("Submitted");
        detail.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()).ShouldBe(["Close"]);
    }

    [Fact]
    public async Task Submit_PublishesTheIntegrationEventWithTheBusinessData()
    {
        using var alice = ClientFor(Alice);
        var (id, version) = await CreateAsync(alice, "事件测试");
        var number = (await JsonAsync(await alice.GetAsync($"{Root}/{id}", Ct))).GetProperty("number").GetString();

        (await PostActionAsync(alice, $"{Root}/{id}/submit", new { expectedVersion = version })).EnsureSuccessStatusCode();

        // 事件和提交在同一个事务里写进本模块的 Outbox，不需要等投递
        var payloads = await OutboxPayloadsAsync("Sample.SpecimenSubmitted", Ct);
        var payload = payloads.Where(p => p.Contains(id.ToString(), StringComparison.OrdinalIgnoreCase)).ShouldHaveSingleItem();
        using var message = JsonDocument.Parse(payload);
        message.RootElement.GetProperty("specimenId").GetGuid().ShouldBe(id);
        message.RootElement.GetProperty("number").GetString().ShouldBe(number);
    }

    [Fact]
    public async Task Submit_WithAStaleVersion_Returns409WithTheCurrentVersion()
    {
        using var alice = ClientFor(Alice);
        var (id, version) = await CreateAsync(alice, "冲突测试");
        (await PostActionAsync(alice, $"{Root}/{id}/submit", new { expectedVersion = version })).EnsureSuccessStatusCode();

        var stale = await PostActionAsync(alice, $"{Root}/{id}/submit", new { expectedVersion = version });

        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var body = await JsonAsync(stale);
        body.GetProperty("code").GetString().ShouldBe("Common.VersionConflict");
        body.GetProperty("currentVersion").GetInt64().ShouldBeGreaterThan(version);
    }

    [Fact]
    public async Task Submit_BySomeoneWhoIsNotTheOwner_Returns403NotOwner()
    {
        using var alice = ClientFor(Alice);
        using var bob = ClientFor(Bob);
        var (id, version) = await CreateAsync(alice, "负责人测试");

        // 乙的数据范围是全部，看得到，也有提交权限，但不是负责人
        var response = await PostActionAsync(bob, $"{Root}/{id}/submit", new { expectedVersion = version });

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Sample.NotOwner");
    }

    [Fact]
    public async Task DataOutsideTheScope_LooksLikeItDoesNotExist()
    {
        using var alice = ClientFor(Alice);
        using var carol = ClientFor(Carol);
        var (id, _) = await CreateAsync(alice, "范围测试");

        var detail = await carol.GetAsync($"{Root}/{id}", Ct);

        detail.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(detail)).GetProperty("code").GetString().ShouldBe("Sample.NotFound");
    }

    [Fact]
    public async Task DataOutsideTheScope_IsRecordedAsASecurityEvent()
    {
        using var alice = ClientFor(Alice);
        using var carol = ClientFor(Carol);
        var (id, _) = await CreateAsync(alice, "越权记录");

        (await carol.GetAsync($"{Root}/{id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // 安全事件由后台循环批量写库，不在请求线程里，所以要等
        var recorded = await Eventually.UntilAsync(
            ct => CountOutOfScopeEventsAsync(id, ct), count => count > 0, "范围外访问的安全事件写入数据库", ct: Ct);
        recorded.ShouldBe(1);
    }

    [Fact]
    public async Task List_ShowsOnlyDataInTheScope_WithPagingAndSorting()
    {
        using var alice = ClientFor(Alice);
        using var carol = ClientFor(Carol);
        foreach (var title in new[] { "列表乙", "列表丙", "列表甲" })
        {
            await CreateAsync(alice, title);
        }

        await CreateAsync(carol, "列表别人的");

        var page = await JsonAsync(await alice.GetAsync($"{Root}?q=列表&sort=title&pageSize=2", Ct));

        page.GetProperty("totalCount").GetInt64().ShouldBe(3);
        page.GetProperty("totalCountExact").GetBoolean().ShouldBeTrue();
        page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()).ShouldBe(["列表丙", "列表乙"]);

        var second = await JsonAsync(await alice.GetAsync($"{Root}?q=列表&sort=title&pageSize=2&page=2", Ct));
        second.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()).ShouldBe(["列表甲"]);
    }

    [Fact]
    public async Task List_WithAnUnknownSortField_Returns400()
    {
        using var alice = ClientFor(Alice);

        var response = await alice.GetAsync($"{Root}?sort=secret", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(response)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("sort");
    }

    [Fact]
    public async Task Delete_RemovesADraft_AndRefusesASubmittedOne()
    {
        using var alice = ClientFor(Alice);
        var (draft, draftVersion) = await CreateAsync(alice, "删除测试");
        var (submitted, submittedVersion) = await CreateAsync(alice, "已提交不能删");
        (await PostActionAsync(alice, $"{Root}/{submitted}/submit", new { expectedVersion = submittedVersion })).EnsureSuccessStatusCode();
        var newVersion = (await JsonAsync(await alice.GetAsync($"{Root}/{submitted}", Ct))).GetProperty("version").GetInt64();

        var deleted = await DeleteAsync(alice, draft, draftVersion);
        var refused = await DeleteAsync(alice, submitted, newVersion);

        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await alice.GetAsync($"{Root}/{draft}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(refused)).GetProperty("code").GetString().ShouldBe("Sample.InvalidState");
    }

    [Fact]
    public async Task EveryResponse_HasATraceIdHeader()
    {
        using var alice = ClientFor(Alice);

        var response = await alice.GetAsync($"{Root}/{Guid.CreateVersion7()}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await JsonAsync(response);
        response.Headers.GetValues("X-Trace-Id").ShouldHaveSingleItem().ShouldBe(body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task HealthChecks_AreAnonymous_AndReportReady()
    {
        using var anonymous = ClientFor();

        var live = await anonymous.GetAsync("/health/live", Ct);
        var ready = await anonymous.GetAsync("/health/ready", Ct);

        live.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await live.Content.ReadAsStringAsync(Ct)).ShouldBe("Healthy");
        ready.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static Task<HttpResponseMessage> DeleteAsync(HttpClient client, Guid id, long version)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Root}/{id}") { Content = JsonContent.Create(new { expectedVersion = version }) };
        return client.SendAsync(request, Ct);
    }

    private static async Task<HttpResponseMessage> PostCreateAsync(HttpClient client, string? key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Root)
        {
            Content = JsonContent.Create(new { title = "标本", items = new[] { new { name = "样品", quantity = 2 } } }),
        };
        if (key is not null)
        {
            request.Headers.Add("Idempotency-Key", key);
        }

        return await client.SendAsync(request, Ct);
    }

    private async Task<long> CountOutOfScopeEventsAsync(Guid resourceId, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            """SELECT count(*) FROM "Identity"."SecurityEvent" WHERE "Type" = 'AuthorizationDenied' AND "Reason" = 'OutOfDataScope' AND "ResourceId" = @id""",
            connection);
        command.Parameters.AddWithValue("id", resourceId);
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    // 同一个类的测试共用一个库，库里有别的测试提交的标本，所以取同类型的全部载荷，由调用方挑自己的
    private async Task<List<string>> OutboxPayloadsAsync(string messageType, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand("""SELECT "Payload" FROM "Sample"."OutboxMessage" WHERE "MessageType" = @type""", connection);
        command.Parameters.AddWithValue("type", messageType);
        var payloads = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            payloads.Add(reader.GetString(0));
        }

        return payloads;
    }
}
