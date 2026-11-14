using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Files.Application;
using Kmic.Files.Application.Cleanup;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 对账：存储里多出的对象被删除，库里缺对象的文件记为存储问题；以及存储问题的查看与标记已解决
/// </summary>
/// <remarks>
/// 用自己的存储目录，别的测试类放进去的对象不会被对账碰到。对象的年龄看文件 ID 里的时间，
/// 所以“老对象”用指定时间生成的 ID 来造
/// </remarks>
public sealed class FileReconciliationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(
        database,
        template,
        storageRoot: _root,
        moreSettings: new Dictionary<string, string>(StringComparer.Ordinal) { ["Files:Storage:Environment"] = Environment },
        configureServices: services => VersionedInventory.Install(services)),
    IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Environment = "rec";
    private const string Root = "/api/v1/files";
    private const string VersionStillThere = "v-still-there";

    private static readonly string _root = Path.Combine(Path.GetTempPath(), "kmic-reconcile-tests-" + Guid.CreateVersion7().ToString("N"));
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private TestIdentity Admin() => Register("对账管理员", FilesPermissions.FileView, FilesPermissions.FileManage);

    private TestIdentity Viewer() => Register("对账查看员", FilesPermissions.FileView);

    private TestIdentity Register(string name, params string[] permissions)
    {
        var identity = TestIdentity.User(name).InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));
        Factory.Loader.Register(identity);
        return identity;
    }

    private static string PathOf(string key) => Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));

    // 在存储里放一个对象，ID 里的时间是 daysOld 天前
    private static async Task<(Guid Id, string Key)> PutObjectAsync(int daysOld)
    {
        var id = Guid.CreateVersion7(TimeProvider.System.GetUtcNow().AddDays(-daysOld));
        var key = FileStorageKeys.For(Environment, id, TimeProvider.System.GetUtcNow().AddDays(-daysOld));
        Directory.CreateDirectory(Path.GetDirectoryName(PathOf(key))!);
        await File.WriteAllTextAsync(PathOf(key), "内容", Ct);
        return (id, key);
    }

    private async Task<Result<ScheduledJobOutcome>> RunAsync()
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.ScheduledJob);
        return await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ReconcileStorageCommand(TimeProvider.System.GetUtcNow()), Ct);
    }

    // 上传一个文件，把上传时间改老、对象删掉：库里有记录，存储里没有对象
    private async Task<(Guid Id, string Key)> FileWithoutObjectAsync(IdentitySeed seed, string name)
    {
        using var alice = ClientFor(Alice);
        var id = await UploadOkAsync(alice, name, Pdf());
        var key = (await ColumnAsync<string>(seed, id, "StorageKey"))!;
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "UploadedAt" = now() - interval '3 days' WHERE "Id" = @id""", ("id", id));
        File.Delete(PathOf(key));
        return (id, key);
    }

    private static Task<long> IssuesOfAsync(IdentitySeed seed, Guid fileId) =>
        seed.CountAsync($"""SELECT count(*) FROM "Files"."StorageIssue" WHERE "FileId" = '{fileId}'""");

    [Fact]
    public async Task Orphans_OlderThanTwoDays_AreDeleted_ButNewOnesAndForeignObjectsStay()
    {
        var (_, oldKey) = await PutObjectAsync(daysOld: 3);
        var (_, recentKey) = await PutObjectAsync(daysOld: 0);
        var foreignKey = $"{Environment}/readme.txt";
        await File.WriteAllTextAsync(PathOf(foreignKey), "别的程序放的", Ct);

        var outcome = (await RunAsync()).ShouldBeSuccess();

        outcome.ItemsProcessed.ShouldBeGreaterThanOrEqualTo(1);
        File.Exists(PathOf(oldKey)).ShouldBeFalse("存在三天、库里没有记录");
        File.Exists(PathOf(recentKey)).ShouldBeTrue("还不到两天，可能还在写入或等登记");
        File.Exists(PathOf(foreignKey)).ShouldBeTrue("键不是 Files 的写法，不是我们的对象");
    }

    [Fact]
    public async Task Orphan_WithAnUnfinishedRegisterRequest_IsKeptUntilTheRequestIsFinished()
    {
        await using var seed = NewSeed();
        var (id, key) = await PutObjectAsync(daysOld: 3);
        var message = Guid.CreateVersion7();
        await seed.SqlAsync(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "SubjectIds", "OccurredAt", "Status", "Attempts")
            VALUES (@message, 'Files.RegisterGeneratedFile', 1, 0, '{}'::jsonb, '{}'::jsonb, ARRAY[@file]::uuid[], now(), 3, 8)
            """,
            ("message", message), ("file", id));

        (await RunAsync()).ShouldBeSuccess();
        File.Exists(PathOf(key)).ShouldBeTrue("登记请求还没处理完，对象不是多余的");

        await seed.SqlAsync("""UPDATE "Sample"."OutboxMessage" SET "Status" = 2 WHERE "Id" = @message""", ("message", message));
        (await RunAsync()).ShouldBeSuccess();
        File.Exists(PathOf(key)).ShouldBeFalse();
    }

    [Fact]
    public async Task FileWhoseObjectIsGone_IsRejectedAsLost_AndAnIssueIsRecordedOnlyOnce()
    {
        await using var seed = NewSeed();
        var (id, _) = await FileWithoutObjectAsync(seed, "丢了的.pdf");

        (await RunAsync()).ShouldBeSuccess().ItemsProcessed.ShouldBeGreaterThanOrEqualTo(1);
        (await RunAsync()).ShouldBeSuccess();

        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Rejected");
        (await ColumnAsync<string>(seed, id, "RejectReason")).ShouldBe("对象丢失");
        (await seed.ScalarAsync<string>($"""SELECT "Kind" FROM "Files"."StorageIssue" WHERE "FileId" = '{id}'""")).ShouldBe("LostMissing");
        (await IssuesOfAsync(seed, id)).ShouldBe(1);
        (await seed.CountAsync($"""SELECT count(*) FROM "Files"."StoredFileStateHistory" WHERE "AggregateId" = '{id}' AND "ActionCode" = 'Lose'""")).ShouldBe(1);
    }

    [Fact]
    public async Task FileWhoseOldVersionIsStillThere_IsOnlyRecordedAsRestorable_AndStaysAsItIs()
    {
        await using var seed = NewSeed();
        var (id, _) = await FileWithoutObjectAsync(seed, "可以还原.pdf");
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ObjectVersionId" = @version WHERE "Id" = @id""", ("version", VersionStillThere), ("id", id));

        (await RunAsync()).ShouldBeSuccess();
        (await RunAsync()).ShouldBeSuccess();

        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available", "等运维还原，文件不动");
        (await seed.ScalarAsync<string>($"""SELECT "Kind" FROM "Files"."StorageIssue" WHERE "FileId" = '{id}'""")).ShouldBe("RestorableMissing");
        (await IssuesOfAsync(seed, id)).ShouldBe(1, "已经有待处理的问题，不重复记");
    }

    [Fact]
    public async Task Issues_AreListedWithFileName_AndCanBeResolvedByThoseWhoManage()
    {
        await using var seed = NewSeed();
        var (id, _) = await FileWithoutObjectAsync(seed, "待还原的.pdf");
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ObjectVersionId" = @version WHERE "Id" = @id""", ("version", VersionStillThere), ("id", id));
        (await RunAsync()).ShouldBeSuccess();
        var adminIdentity = Admin();
        using var admin = ClientFor(adminIdentity);
        using var viewer = ClientFor(Viewer());

        var list = await JsonAsync(await admin.GetAsync($"{Root}/storage-issues?status=Open", Ct));
        var issue = list.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("fileId").GetGuid() == id);
        var issueId = issue.GetProperty("id").GetGuid();
        var version = issue.GetProperty("version").GetInt64();

        issue.GetProperty("fileName").GetString().ShouldBe("待还原的.pdf");
        issue.GetProperty("kind").GetString().ShouldBe("RestorableMissing");
        issue.GetProperty("objectVersionId").GetString().ShouldBe(VersionStillThere);
        (await viewer.GetAsync($"{Root}/storage-issues", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var denied = await PostActionAsync(viewer, $"{Root}/storage-issues/{issueId}/resolve", new { note = "试一下", expectedVersion = version });
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var stale = await PostActionAsync(admin, $"{Root}/storage-issues/{issueId}/resolve", new { note = "已还原", expectedVersion = version + 5 });
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var resolved = await PostActionAsync(admin, $"{Root}/storage-issues/{issueId}/resolve", new { note = "工单 1234 已还原", expectedVersion = version });
        resolved.StatusCode.ShouldBe(HttpStatusCode.OK);

        var open = await JsonAsync(await admin.GetAsync($"{Root}/storage-issues?status=Open", Ct));
        open.GetProperty("items").EnumerateArray().ShouldNotContain(i => i.GetProperty("id").GetGuid() == issueId);
        var all = await JsonAsync(await admin.GetAsync($"{Root}/storage-issues?status=Resolved", Ct));
        var done = all.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("id").GetGuid() == issueId);
        done.GetProperty("resolutionNote").GetString().ShouldBe("工单 1234 已还原");
        done.GetProperty("resolvedBy").GetGuid().ShouldBe(adminIdentity.UserId.Value);
    }

    [Fact]
    public async Task ResolveWithoutANote_IsRejected()
    {
        await using var seed = NewSeed();
        var (id, _) = await FileWithoutObjectAsync(seed, "没写说明.pdf");
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ObjectVersionId" = @version WHERE "Id" = @id""", ("version", VersionStillThere), ("id", id));
        (await RunAsync()).ShouldBeSuccess();
        using var admin = ClientFor(Admin());
        var list = await JsonAsync(await admin.GetAsync($"{Root}/storage-issues?status=Open", Ct));
        var issue = list.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("fileId").GetGuid() == id);

        var response = await PostActionAsync(admin, $"{Root}/storage-issues/{issue.GetProperty("id").GetGuid()}/resolve", new { note = " ", expectedVersion = issue.GetProperty("version").GetInt64() });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 把“版本 v-still-there 还在”告诉对账：本地存储没有版本，用它模拟开了版本控制的桶里被删除标记盖住的对象
    /// </summary>
    private sealed class VersionedInventory(IStorageInventory inner) : IStorageInventory
    {
        public static void Install(IServiceCollection services)
        {
            var original = services.Single(d => d.ServiceType == typeof(IStorageInventory));
            services.Remove(original);
            services.AddSingleton<IStorageInventory>(provider =>
                new VersionedInventory((IStorageInventory)ActivatorUtilities.CreateInstance(provider, original.ImplementationType!)));
        }

        public IAsyncEnumerable<InventoryObject> ListAsync(CancellationToken ct) => inner.ListAsync(ct);

        public Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct) =>
            string.Equals(versionId, VersionStillThere, StringComparison.Ordinal) ? Task.FromResult(true) : inner.ExistsAsync(key, versionId, ct);

        public Task DeleteAsync(string key, CancellationToken ct) => inner.DeleteAsync(key, ct);
    }
}
