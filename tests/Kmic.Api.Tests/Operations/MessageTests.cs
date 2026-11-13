using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 消息管理：积压汇总、死信列表与内容、被挡住的键、重试与放弃（含接收方的放弃检查），端到端。死信行直接写进各模块的收件箱与发件箱
/// </summary>
public sealed class MessageTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>,
        IDisposable
{
    private const string Root = "/api/v1/operations/messages";
    private const string EmptyObject = "{}";
    private const short Pending = 0;
    private const short Done = 2;
    private const short Dead = 3;
    private const short Abandoned = 4;

    private static readonly string[] _view = [OperationsPermissions.MessagesView];
    private static readonly string[] _manage = [OperationsPermissions.MessagesView, OperationsPermissions.MessagesManage];

    private readonly string _unique = Guid.CreateVersion7().ToString("N");
    private readonly DateTimeOffset _base = TimeProvider.System.GetUtcNow().AddHours(-3);
    private WebApplicationFactory<Program>? _withPolicy;
    private long _seq = 1;

    public void Dispose() => _withPolicy?.Dispose();

    private async Task<Guid> InsertInboxAsync(string module, string subscription, short status, string type, DateTimeOffset occurred, string? error = null, string payload = "{}", string? key = null)
    {
        var id = Guid.CreateVersion7();
        var errorText = error is null ? "NULL" : $"'{error}'";
        var keyText = key is null ? "NULL" : $"'{key}'";
        await Seed.SqlAsync(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""
                INSERT INTO "{module}"."InboxMessage" ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers", "SourceModule", "OrderingKey", "Seq", "OccurredAt", "ReceivedAt", "Status", "Attempts", "LastError")
                VALUES ('{id}', '{Guid.CreateVersion7()}', '{subscription}', '{type}', 1, '{payload}'::jsonb, '{EmptyObject}'::jsonb, 'Probe', {keyText}, {_seq++}, '{occurred:O}', '{occurred:O}', {status}, 10, {errorText})
                """));
        return id;
    }

    private async Task<Guid> InsertOutboxAsync(string module, short status, string type, DateTimeOffset occurred, string? error = null, string payload = "{}")
    {
        var id = Guid.CreateVersion7();
        var errorText = error is null ? "NULL" : $"'{error}'";
        await Seed.SqlAsync(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""
                INSERT INTO "{module}"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "OccurredAt", "Status", "Attempts", "LastError")
                VALUES ('{id}', '{type}', 1, 0, '{payload}'::jsonb, '{EmptyObject}'::jsonb, '{occurred:O}', {status}, 10, {errorText})
                """));
        return id;
    }

    private Task<short> StatusOfAsync(string module, string box, Guid id) =>
        Seed.ScalarAsync<short>($"""SELECT "Status" FROM "{module}"."{box}Message" WHERE "Id" = '{id}'""");

    private async Task<string> LoginAsync(string account, IReadOnlyCollection<string> permissions) => await LoginAsOperatorAsync(account, permissions);

    private Task<HttpResponseMessage> GetAsync(string token, string path) => SendAsync(HttpMethod.Get, Root + path, token);

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task TheSummary_CountsEachModulesBoxesAndFlagsRetiringSubscriptions()
    {
        var viewer = await LoginAsync("msg-summary-viewer", _view);
        var plain = await LoginAsync("msg-summary-plain", []);
        var active = "Sample.ActiveSub" + _unique;
        var retiring = "Sample.RetiringSub" + _unique;
        await InsertInboxAsync("Sample", active, Pending, "Test.Summary", _base, payload: "{}");
        await InsertInboxAsync("Sample", active, Dead, "Test.Summary", _base.AddMinutes(1), error: "出错");
        await InsertInboxAsync("Sample", retiring, Pending, "Test.Summary", _base.AddMinutes(2));
        await InsertOutboxAsync("Configuration", Dead, "Test.Summary", _base);
        await Seed.SqlAsync(
            $"""
            INSERT INTO "Operations"."MessageRoute" ("MessageType", "Subscription", "Module", "Status", "RegisteredBuild")
            VALUES ('Test.Summary', '{retiring}', 'Sample', 1, '1.0.0')
            """);

        var response = await GetAsync(viewer, "/summary");
        var forbidden = await GetAsync(plain, "/summary");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var modules = (await JsonAsync(response)).GetProperty("modules").EnumerateArray().ToList();
        modules.Select(m => m.GetProperty("module").GetString()).ShouldBe(modules.Select(m => m.GetProperty("module").GetString()).Order(StringComparer.Ordinal));
        var sample = modules.Single(m => string.Equals(m.GetProperty("module").GetString(), "Sample", StringComparison.Ordinal));
        var activeItem = sample.GetProperty("subscriptions").EnumerateArray().Single(s => string.Equals(s.GetProperty("subscription").GetString(), active, StringComparison.Ordinal));
        activeItem.GetProperty("pending").GetInt64().ShouldBe(1);
        activeItem.GetProperty("dead").GetInt64().ShouldBe(1);
        activeItem.GetProperty("oldestAgeSeconds").GetInt64().ShouldBeGreaterThan(2 * 3600);
        activeItem.GetProperty("isRetiring").GetBoolean().ShouldBeFalse();
        sample.GetProperty("subscriptions").EnumerateArray().Single(s => string.Equals(s.GetProperty("subscription").GetString(), retiring, StringComparison.Ordinal))
            .GetProperty("isRetiring").GetBoolean().ShouldBeTrue("待终结订阅的积压是预期内的，要带标记");
        modules.Single(m => string.Equals(m.GetProperty("module").GetString(), "Configuration", StringComparison.Ordinal)).GetProperty("outbox").GetProperty("dead").GetInt64().ShouldBeGreaterThanOrEqualTo(1);
        modules.Any(m => string.Equals(m.GetProperty("module").GetString(), "Operations", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [Fact]
    public async Task TheDeadList_MergesModulesAndBoxesNewestFirst_PagesAndFilters_ButNeverShowsThePayload()
    {
        var viewer = await LoginAsync("msg-list-viewer", _view);
        var plain = await LoginAsync("msg-list-plain", []);
        var type = "Test.List" + _unique;
        var subscription = "Sample.ListSub" + _unique;
        var sampleInbox = await InsertInboxAsync("Sample", subscription, Dead, type, _base.AddMinutes(10), error: "第一个错误", payload: """{"content":"内容"}""");
        var configurationOutbox = await InsertOutboxAsync("Configuration", Dead, type, _base.AddMinutes(20));
        var tie = _base.AddMinutes(30);
        var sampleOutbox = await InsertOutboxAsync("Sample", Dead, type, tie);
        var configurationInbox = await InsertInboxAsync("Configuration", "Configuration.ListSub" + _unique, Dead, type, tie);
        await InsertInboxAsync("Sample", subscription, Pending, type, _base.AddMinutes(40));
        await InsertInboxAsync("Sample", subscription, Abandoned, type, _base.AddMinutes(50));

        var all = await WalkAsync(viewer, $"/dead?messageType={type}", limit: 200);
        var walked = await WalkAsync(viewer, $"/dead?messageType={type}", limit: 1);
        var inboxOnly = await WalkAsync(viewer, $"/dead?messageType={type}&box=Inbox", limit: 50);
        var configurationOnly = await WalkAsync(viewer, $"/dead?messageType={type}&module=Configuration", limit: 50);
        var bySubscription = await WalkAsync(viewer, $"/dead?messageType={type}&subscription={subscription}", limit: 50);
        var badBox = await GetAsync(viewer, "/dead?box=Sideways");
        var forbidden = await GetAsync(plain, "/dead");

        all.Count.ShouldBe(4, "只有死信：待处理的、已放弃的都不在里面");
        all.Select(m => m.GetProperty("occurredAt").GetDateTimeOffset()).ShouldBe(all.Select(m => m.GetProperty("occurredAt").GetDateTimeOffset()).OrderByDescending(t => t));
        walked.Select(m => m.GetProperty("id").GetGuid()).ShouldBe(all.Select(m => m.GetProperty("id").GetGuid()), "每页一条翻完，和一次取完一样，同一时间的两条一条也不能漏");
        all.Select(m => m.GetProperty("id").GetGuid()).ShouldBe(
            [.. new[] { sampleOutbox, configurationInbox }.OrderByDescending(id => id.ToByteArray(bigEndian: true), ByteOrder.Instance), configurationOutbox, sampleInbox],
            "同一时间按数据库的 uuid 次序，大的在前");
        inboxOnly.Select(m => m.GetProperty("id").GetGuid()).ShouldBe([.. new[] { configurationInbox, sampleInbox }.OrderByDescending(id => id.ToByteArray(bigEndian: true), ByteOrder.Instance)]);
        configurationOnly.Select(m => m.GetProperty("module").GetString()).ShouldAllBe(m => string.Equals(m, "Configuration", StringComparison.Ordinal));
        bySubscription.Select(m => m.GetProperty("id").GetGuid()).ShouldBe([sampleInbox]);
        all.Single(m => m.GetProperty("id").GetGuid() == sampleInbox).GetProperty("lastError").GetString().ShouldBe("第一个错误");
        all.Single(m => m.GetProperty("id").GetGuid() == sampleInbox).GetProperty("box").GetString().ShouldBe("Inbox");
        all.Where(m => m.TryGetProperty("payload", out _)).ShouldBeEmpty("列表里没有载荷");
        badBox.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(badBox)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("box");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TheDeadMessage_ShowsPayloadAndHeaders_AndAnUnknownOneIsNotFound()
    {
        var viewer = await LoginAsync("msg-get-viewer", _view);
        var plain = await LoginAsync("msg-get-plain", []);
        var type = "Test.Get" + _unique;
        var dead = await InsertInboxAsync("Sample", "Sample.GetSub" + _unique, Dead, type, _base, error: "载荷里有问题", payload: """{"quantity":3,"name":"样品"}""");
        var abandoned = await InsertInboxAsync("Sample", "Sample.GetSub" + _unique, Abandoned, type, _base);
        var pending = await InsertInboxAsync("Sample", "Sample.GetSub" + _unique, Pending, type, _base);
        var outbox = await InsertOutboxAsync("Configuration", Dead, type, _base, payload: """{"outbox":true}""");

        var deadResponse = await GetAsync(viewer, $"/dead/Sample/Inbox/{dead}");
        var abandonedResponse = await GetAsync(viewer, $"/dead/Sample/Inbox/{abandoned}");
        var outboxResponse = await GetAsync(viewer, $"/dead/Configuration/Outbox/{outbox}");
        var pendingResponse = await GetAsync(viewer, $"/dead/Sample/Inbox/{pending}");
        var wrongBox = await GetAsync(viewer, $"/dead/Sample/Outbox/{dead}");
        var unknownModule = await GetAsync(viewer, $"/dead/NoSuchModule/Inbox/{dead}");
        var badBox = await GetAsync(viewer, $"/dead/Sample/Sideways/{dead}");
        var forbidden = await GetAsync(plain, $"/dead/Sample/Inbox/{dead}");

        deadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await JsonAsync(deadResponse);
        detail.GetProperty("state").GetString().ShouldBe("Dead");
        detail.GetProperty("payload").GetProperty("quantity").GetInt32().ShouldBe(3);
        detail.GetProperty("payload").GetProperty("name").GetString().ShouldBe("样品");
        detail.GetProperty("headers").ValueKind.ShouldBe(JsonValueKind.Object);
        detail.GetProperty("message").GetProperty("lastError").GetString().ShouldBe("载荷里有问题");
        (await JsonAsync(abandonedResponse)).GetProperty("state").GetString().ShouldBe("Abandoned");
        (await JsonAsync(outboxResponse)).GetProperty("payload").GetProperty("outbox").GetBoolean().ShouldBeTrue();
        foreach (var missing in new[] { pendingResponse, wrongBox, unknownModule })
        {
            missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await CodeOfAsync(missing)).ShouldBe("Operations.MessageNotFound");
        }

        badBox.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BlockedKeys_NeedThePermission_AndAreEmptyWhenNoOrderedSubscriptionHasADeadRow()
    {
        var viewer = await LoginAsync("msg-blocked-viewer", _view);
        var plain = await LoginAsync("msg-blocked-plain", []);

        var response = await GetAsync(viewer, "/blocked-keys");
        var one = await GetAsync(viewer, "/blocked-keys?module=Sample");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).ValueKind.ShouldBe(JsonValueKind.Array);
        (await JsonAsync(one)).GetArrayLength().ShouldBe(0);
        (await GetAsync(plain, "/blocked-keys")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Retry_PutsDeadMessagesBack_GivesEachItsOwnReceipt_AndNeedsTheManagePermission()
    {
        var manager = await LoginAsync("msg-retry-manager", _manage);
        var viewer = await LoginAsync("msg-retry-viewer", _view);
        var type = "Test.Retry" + _unique;
        var inbox = await InsertInboxAsync("Sample", "Sample.RetrySub" + _unique, Dead, type, _base);
        var outbox = await InsertOutboxAsync("Configuration", Dead, type, _base);
        var done = await InsertInboxAsync("Sample", "Sample.RetrySub" + _unique, Done, type, _base);
        var missing = Guid.CreateVersion7();

        var response = await SendAsync(
            HttpMethod.Post,
            Root + "/retry",
            manager,
            new
            {
                messages = new object[]
                {
                    new { module = "Sample", box = "Inbox", id = inbox },
                    new { module = "Configuration", box = "Outbox", id = outbox },
                    new { module = "Sample", box = "Inbox", id = done },
                    new { module = "Sample", box = "Inbox", id = missing },
                    new { module = "NoSuchModule", box = "Inbox", id = missing },
                },
            });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var receipts = (await JsonAsync(response)).GetProperty("receipts").EnumerateArray().ToList();
        receipts.Select(r => r.GetProperty("status").GetString()).ShouldBe(["Done", "Done", "WrongState", "NotFound", "NotFound"]);
        receipts.Select(r => r.GetProperty("id").GetGuid()).ShouldBe([inbox, outbox, done, missing, missing]);
        (await StatusOfAsync("Sample", "Inbox", inbox)).ShouldBe(Pending);
        (await StatusOfAsync("Configuration", "Outbox", outbox)).ShouldBe(Pending);
        (await StatusOfAsync("Sample", "Inbox", done)).ShouldBe(Done);
        var forbidden = await SendAsync(HttpMethod.Post, Root + "/retry", viewer, new { messages = new[] { new { module = "Sample", box = "Inbox", id = inbox } } });
        var empty = await SendAsync(HttpMethod.Post, Root + "/retry", manager, new { messages = Array.Empty<object>() });
        var tooMany = await SendAsync(HttpMethod.Post, Root + "/retry", manager, new { messages = Enumerable.Range(0, 201).Select(_ => new { module = "Sample", box = "Inbox", id = Guid.CreateVersion7() }).ToArray() });
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        empty.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        tooMany.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Abandon_NeedsAReason_OnlyAppliesToTheInbox_AndTheReceiversPolicyCanRefuseIt()
    {
        var manager = await LoginAsync("msg-abandon-manager", _manage);
        var viewer = await LoginAsync("msg-abandon-viewer", _view);
        var plainDead = await InsertInboxAsync("Sample", "Sample.AbandonSub" + _unique, Dead, "Framework.AuditRecorded", _base, error: "老错误", payload: AuditPayload("Plain.Action"));
        var pending = await InsertInboxAsync("Sample", "Sample.AbandonSub" + _unique, Pending, "Framework.AuditRecorded", _base, payload: AuditPayload("Plain.Action"));
        var guarded = await InsertInboxAsync("Operations", "Operations.RecordAudit", Dead, "Framework.AuditRecorded", _base, error: "审计落库失败", payload: AuditPayload("Guard.Refuse"));
        var allowed = await InsertInboxAsync("Operations", "Operations.RecordAudit", Dead, "Framework.AuditRecorded", _base, payload: AuditPayload("Guard.Allow"));
        var unreadable = await InsertInboxAsync("Operations", "Operations.RecordAudit", Dead, "Framework.AuditRecorded", _base, payload: """{"auditId":"不是 ID"}""");
        using var client = ClientWithPolicy(manager);

        var response = await PostAsync(
            client,
            "abandon",
            new
            {
                reason = "对方已经不存在，没有必要再处理",
                messages = new object[]
                {
                    new { module = "Sample", box = "Inbox", id = plainDead },
                    new { module = "Sample", box = "Inbox", id = pending },
                    new { module = "Operations", box = "Inbox", id = guarded },
                    new { module = "Operations", box = "Inbox", id = allowed },
                    new { module = "Operations", box = "Inbox", id = unreadable },
                    new { module = "Sample", box = "Inbox", id = Guid.CreateVersion7() },
                },
            });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var receipts = (await JsonAsync(response)).GetProperty("receipts").EnumerateArray().ToList();
        receipts.Select(r => r.GetProperty("status").GetString()).ShouldBe(["Done", "WrongState", "Refused", "Done", "Refused", "NotFound"]);
        receipts[2].GetProperty("reason").GetString().ShouldBe("引用的文件还没有归属，只能重试");
        receipts[4].GetProperty("reason").GetString().ShouldBe(CommonErrors.MessagePayloadUnreadable.Message);
        (await StatusOfAsync("Sample", "Inbox", plainDead)).ShouldBe(Abandoned);
        (await StatusOfAsync("Sample", "Inbox", pending)).ShouldBe(Pending);
        (await StatusOfAsync("Operations", "Inbox", guarded)).ShouldBe(Dead, "接收方不让放弃，状态没动");
        (await StatusOfAsync("Operations", "Inbox", allowed)).ShouldBe(Abandoned);
        (await StatusOfAsync("Operations", "Inbox", unreadable)).ShouldBe(Dead);
        var recorded = await Seed.ScalarAsync<string>($"""SELECT "LastError" FROM "Sample"."InboxMessage" WHERE "Id" = '{plainDead}'""");
        recorded.ShouldNotBeNull().ShouldContain("对方已经不存在，没有必要再处理");
        recorded.ShouldContain("老错误");

        var noReason = await PostAsync(client, "abandon", new { reason = " ", messages = new[] { new { module = "Sample", box = "Inbox", id = plainDead } } });
        var outboxBox = await PostAsync(client, "abandon", new { reason = "试试发件箱", messages = new[] { new { module = "Sample", box = "Outbox", id = plainDead } } });
        var forbidden = await SendAsync(HttpMethod.Post, Root + "/abandon", viewer, new { reason = "没有权限", messages = new[] { new { module = "Sample", box = "Inbox", id = plainDead } } });
        noReason.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        outboxBox.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static string AuditPayload(string action) =>
        JsonSerializer.Serialize(
            new AuditRecorded
            {
                AuditId = Guid.CreateVersion7(),
                OccurredAt = DateTimeOffset.UnixEpoch,
                Module = "Sample",
                Action = action,
                Origin = "Http",
                TraceId = "0123456789abcdef0123456789abcdef",
                Targets = [],
                Changes = [],
                Details = new Dictionary<string, string>(StringComparer.Ordinal),
            },
            KmicJsonOptions.Stored);

    // 接收方的放弃检查是各模块自己注册的；这里的宿主里没有模块注册，所以给审计消息配一个按动作拒绝的
    private HttpClient ClientWithPolicy(string token)
    {
        _withPolicy ??= Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IAbandonPolicy<AuditRecorded>>(new RefuseByAction("Guard.Refuse"))));
        var client = _withPolicy.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Root}/{path}") { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        return await client.SendAsync(request, Ct);
    }

    private async Task<List<JsonElement>> WalkAsync(string token, string path, int limit)
    {
        var seen = new List<JsonElement>();
        string? after = null;
        do
        {
            var response = await GetAsync(token, $"{path}&limit={limit}" + (after is null ? string.Empty : $"&after={Uri.EscapeDataString(after)}"));
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var page = await JsonAsync(response);
            seen.AddRange(page.GetProperty("items").EnumerateArray());
            after = page.GetProperty("nextCursor").ValueKind == JsonValueKind.Null ? null : page.GetProperty("nextCursor").GetString();
        }
        while (after is not null);

        return seen;
    }

    private sealed class RefuseByAction(string action) : IAbandonPolicy<AuditRecorded>
    {
        public ValueTask<Result> CheckAsync(AuditRecorded message, CancellationToken ct) =>
            ValueTask.FromResult(string.Equals(message.Action, action, StringComparison.Ordinal) ? Result.Failure(Error.BusinessRule("Test.StillAttached", "引用的文件还没有归属，只能重试")) : Result.Success());
    }

    // 数据库比较 uuid 是逐字节的
    private sealed class ByteOrder : IComparer<byte[]>
    {
        public static ByteOrder Instance { get; } = new();

        public int Compare(byte[]? x, byte[]? y) => x.AsSpan().SequenceCompareTo(y);
    }
}
