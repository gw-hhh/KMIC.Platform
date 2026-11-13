using System.Net;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Files.Application;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 文件管理：列表、详情、彻底删除、登记历史版本清除、重试删除；权限与结果
/// </summary>
public sealed class FileAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template, withWorker: true), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/files";
    private const string Specimens = "/api/v1/sample/specimens";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private TestIdentity Admin() => Register("文件管理员", FilesPermissions.FileView, FilesPermissions.FileManage, FilesPermissions.FilePurge);

    private TestIdentity Viewer() => Register("文件查看员", FilesPermissions.FileView);

    private TestIdentity Register(string name, params string[] permissions)
    {
        var identity = TestIdentity.User(name).InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));
        Factory.Loader.Register(identity);
        return identity;
    }

    private static async Task<HttpResponseMessage> PurgeAsync(HttpClient client, Guid id, object body) => await PostActionAsync(client, $"{Root}/{id}/purge", body);

    // 建一个标本、上传并关联一个文件，等 Worker 把文件处理成“已关联”
    private async Task<(Guid Specimen, Guid File)> AttachedAsync(string name = "合同.pdf")
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, name, Pdf());
        (await AddAttachmentAsync(alice, specimen, fileId, version)).EnsureSuccessStatusCode();
        await WaitForAsync(fileId, "Attached");
        return (specimen, fileId);
    }

    private async Task WaitForAsync(Guid fileId, string status)
    {
        await using var seed = NewSeed();
        await Eventually.UntilAsync(
            _ => ColumnAsync<string>(seed, fileId, "Status"),
            current => string.Equals(current, status, StringComparison.Ordinal),
            $"文件变成 {status}",
            TimeSpan.FromSeconds(20),
            Ct);
    }

    [Fact]
    public async Task List_ShowsTheFileWithoutItsStorageKey_AndFiltersByStatusNameAndReference()
    {
        using var admin = ClientFor(Admin());
        var (specimen, fileId) = await AttachedAsync("列表里的.pdf");
        using var alice = ClientFor(Alice);
        await UploadOkAsync(alice, "没有关联的.pdf", Pdf());

        var byName = await JsonAsync(await admin.GetAsync($"{Root}?q=列表里的&status=Attached", Ct));
        var item = byName.GetProperty("items").EnumerateArray().Single();
        var byReference = await JsonAsync(await admin.GetAsync($"{Root}?ownerModule=Sample&ownerType=Specimen&ownerId={specimen}", Ct));

        item.GetProperty("id").GetGuid().ShouldBe(fileId);
        item.GetProperty("name").GetString().ShouldBe("列表里的.pdf");
        item.GetProperty("status").GetString().ShouldBe("Attached");
        item.TryGetProperty("storageKey", out _).ShouldBeFalse("管理员也看不到存储键");
        byReference.GetProperty("items").EnumerateArray().Single().GetProperty("id").GetGuid().ShouldBe(fileId);
    }

    [Fact]
    public async Task Detail_ShowsTheReferences_ButNeitherTheStorageKeyNorADownloadAddress()
    {
        using var admin = ClientFor(Admin());
        var (specimen, fileId) = await AttachedAsync();

        var detail = await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct));

        detail.GetProperty("profile").GetString().ShouldBe(Profile);
        var reference = detail.GetProperty("references").EnumerateArray().Single();
        reference.GetProperty("ownerModule").GetString().ShouldBe("Sample");
        reference.GetProperty("ownerType").GetString().ShouldBe("Specimen");
        reference.GetProperty("ownerId").GetGuid().ShouldBe(specimen);
        detail.TryGetProperty("storageKey", out _).ShouldBeFalse();
        detail.EnumerateObject().Select(p => p.Name).ShouldNotContain(n => n.Contains("url", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Admin_Endpoints_NeedTheirPermissions()
    {
        using var viewer = ClientFor(Viewer());
        using var nobody = ClientFor(Bob);
        var (_, fileId) = await AttachedAsync();

        (await viewer.GetAsync($"{Root}/{fileId}", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await nobody.GetAsync($"{Root}/{fileId}", Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var purge = await PurgeAsync(viewer, fileId, new { reason = "试一下", expectedVersion = 0 });
        purge.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(purge)).GetProperty("code").GetString().ShouldBe("Common.MissingPermission");
        (await PostActionAsync(viewer, $"{Root}/{fileId}/retry-deletion", new { })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purge_RemovesTheObject_KeepsTheRecord_AndTheReferencingDocumentSaysItIsGone()
    {
        using var admin = ClientFor(Admin());
        using var alice = ClientFor(Alice);
        var (specimen, fileId) = await AttachedAsync();
        await using var seed = NewSeed();
        var key = (await ColumnAsync<string>(seed, fileId, "StorageKey"))!;
        var attachment = (await JsonAsync(await alice.GetAsync($"{Specimens}/{specimen}", Ct))).GetProperty("attachments").EnumerateArray().First().GetProperty("id").GetGuid();
        var version = (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("version").GetInt64();

        var purged = await PurgeAsync(admin, fileId, new { reason = "误传了敏感文件", expectedVersion = version });

        purged.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct));
        detail.GetProperty("status").GetString().ShouldBe("Purged");
        detail.GetProperty("purgeReason").GetString().ShouldBe("误传了敏感文件");
        detail.GetProperty("references").GetArrayLength().ShouldBe(1, "引用保留，单据才能显示“文件已删除”");
        await Eventually.UntilAsync(
            _ => IsNullAsync(seed, fileId, "ObjectDeletedAt"),
            isNull => !isNull,
            "删除工作器删掉对象并记下时间",
            TimeSpan.FromSeconds(20),
            Ct);
        File.Exists(StoragePathOf(key)).ShouldBeFalse();
        var download = await alice.GetAsync($"{Specimens}/{specimen}/attachments/{attachment}/content", Ct);
        download.StatusCode.ShouldBe(HttpStatusCode.Gone);
        (await JsonAsync(download)).GetProperty("code").GetString().ShouldBe("Files.Purged");
    }

    [Fact]
    public async Task Purge_Again_IsASuccessThatChangesNothing()
    {
        using var admin = ClientFor(Admin());
        var (_, fileId) = await AttachedAsync();
        var version = (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("version").GetInt64();
        var first = await PurgeAsync(admin, fileId, new { reason = "第一次", expectedVersion = version });
        var current = (await JsonAsync(first)).GetProperty("version").GetInt64();

        var second = await PurgeAsync(admin, fileId, new { reason = "第二次", expectedVersion = current });

        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("purgeReason").GetString().ShouldBe("第一次");
    }

    [Fact]
    public async Task Purge_NeedsAReason_AndTheCurrentVersion()
    {
        using var admin = ClientFor(Admin());
        var (_, fileId) = await AttachedAsync();
        var version = (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("version").GetInt64();

        var noReason = await PurgeAsync(admin, fileId, new { reason = "", expectedVersion = version });
        var stale = await PurgeAsync(admin, fileId, new { reason = "版本不对", expectedVersion = version + 5 });

        noReason.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(noReason)).GetProperty("code").GetString().ShouldBe("Common.ValidationFailed");
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(stale)).GetProperty("code").GetString().ShouldBe("Common.VersionConflict");
    }

    [Fact]
    public async Task Purge_WhileAnAttachRequestIsStillOnItsWay_AsksToTryAgainLater()
    {
        using var admin = ClientFor(Admin());
        using var alice = ClientFor(Alice);
        var fileId = await UploadOkAsync(alice, "还没关联完.pdf", Pdf());
        await using var seed = NewSeed();
        await seed.SqlAsync(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "SubjectIds", "OccurredAt", "Status", "Attempts")
            VALUES (@message, 'Files.AttachFiles', 1, 0, '{}'::jsonb, '{}'::jsonb, ARRAY[@file]::uuid[], now(), 3, 8)
            """,
            ("message", Guid.CreateVersion7()), ("file", fileId));
        var version = (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("version").GetInt64();

        var response = await PurgeAsync(admin, fileId, new { reason = "有请求在路上", expectedVersion = version });

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.TransientConflict");
        (await ColumnAsync<string>(seed, fileId, "Status")).ShouldBe("Available");
    }

    [Fact]
    public async Task RecordHistoryPurge_OnlyForAFileThatHasBeenPurged()
    {
        using var admin = ClientFor(Admin());
        var (_, fileId) = await AttachedAsync();
        var detail = await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct));
        var version = detail.GetProperty("version").GetInt64();

        var early = await PostActionAsync(admin, $"{Root}/{fileId}/history-purge", new { note = "工单 1", expectedVersion = version });
        var purged = await PurgeAsync(admin, fileId, new { reason = "彻底删除", expectedVersion = version });
        var afterPurge = (await JsonAsync(purged)).GetProperty("version").GetInt64();
        var recorded = await PostActionAsync(admin, $"{Root}/{fileId}/history-purge", new { note = "工单 2：已在 OSS 清除全部历史版本", expectedVersion = afterPurge });

        early.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(early)).GetProperty("code").GetString().ShouldBe("Files.InvalidState");
        recorded.StatusCode.ShouldBe(HttpStatusCode.OK);
        var after = await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct));
        after.GetProperty("historyPurgeNote").GetString().ShouldBe("工单 2：已在 OSS 清除全部历史版本");
        after.GetProperty("historyPurgedAt").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.String);
    }

    [Fact]
    public async Task RetryDeletion_ResetsAFileThatStoppedBeingRetried()
    {
        using var admin = ClientFor(Admin());
        var (_, fileId) = await AttachedAsync();
        await using var seed = NewSeed();
        // 没有进删除队列的文件没有什么可重试
        var nothing = await PostActionAsync(admin, $"{Root}/{fileId}/retry-deletion", new { });
        nothing.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var version = (await JsonAsync(await admin.GetAsync($"{Root}/{fileId}", Ct))).GetProperty("version").GetInt64();
        (await PurgeAsync(admin, fileId, new { reason = "彻底删除", expectedVersion = version })).EnsureSuccessStatusCode();
        await Eventually.UntilAsync(
            _ => IsNullAsync(seed, fileId, "ObjectDeletedAt"),
            isNull => !isNull,
            "对象先被删掉",
            TimeSpan.FromSeconds(20),
            Ct);

        // 对象已经删了、不在队列里：同样没有可重试的
        (await PostActionAsync(admin, $"{Root}/{fileId}/retry-deletion", new { })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // 模拟“尝试次数用完”的文件：把对象删除时间清空、次数拉满，工作器不再领取；重试之后清零
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ObjectDeletedAt" = NULL, "DeleteAttempts" = 8 WHERE "Id" = @id""", ("id", fileId));
        await Task.Delay(TimeSpan.FromSeconds(1), TimeProvider.System, Ct);
        (await seed.ScalarAsync<int>("""SELECT "DeleteAttempts" FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", fileId))).ShouldBe(8, "用完次数的文件不再被领取");

        var retried = await PostActionAsync(admin, $"{Root}/{fileId}/retry-deletion", new { });

        retried.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await Eventually.UntilAsync(
            _ => IsNullAsync(seed, fileId, "ObjectDeletedAt"),
            isNull => !isNull,
            "重试之后删除工作器重新领取并完成",
            TimeSpan.FromSeconds(20),
            Ct);
    }
}
