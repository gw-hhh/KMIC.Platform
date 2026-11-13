using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 文件的一生：上传、关联、下载、解除关联，关联与解除关联的请求消息由 Worker 真的处理
/// </summary>
public sealed class FileLifecycleTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template, withWorker: true), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimens";

    [Fact]
    public async Task AttachRequest_IsProcessed_TheFileBecomesAttachedWithOneReference()
    {
        using var alice = ClientFor(Alice);
        var (specimen, fileId, _) = await AttachedAsync(alice);
        await using var seed = NewSeed();

        (await seed.CountAsync($"""SELECT count(*) FROM "Files"."FileReference" WHERE "StoredFileId" = '{fileId}' AND "OwnerModule" = 'Sample' AND "OwnerType" = 'Specimen' AND "OwnerId" = '{specimen}'""")).ShouldBe(1);
        (await IsNullAsync(seed, fileId, "ExpiresAt")).ShouldBeTrue("已关联的文件不再是清理候选");
        (await seed.CountAsync($"""SELECT count(*) FROM "Files"."StoredFileStateHistory" WHERE "AggregateId" = '{fileId}'""")).ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Download_OfAnAttachedFile_StreamsTheContentWithSafeHeaders()
    {
        using var alice = ClientFor(Alice);
        var content = Pdf("下载的内容");
        var (specimen, _, attachment) = await AttachedAsync(alice, content);

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync(Ct)).ShouldBe(content);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/pdf");
        response.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        response.Content.Headers.ContentDisposition.FileNameStar.ShouldBe("合同.pdf");
        response.Headers.ETag!.Tag.ShouldBe($"\"{Convert.ToHexStringLower(SHA256.HashData(content))}\"");
        response.Headers.GetValues("X-Content-Type-Options").ShouldBe(["nosniff"]);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
        response.Headers.CacheControl.Private.ShouldBeTrue();
        response.Headers.Contains("Content-Security-Policy").ShouldBeFalse("附件下载不会在浏览器里执行，不需要沙箱");
    }

    [Fact]
    public async Task Download_WithInline_OpensAPdfInTheBrowserInsideASandbox()
    {
        using var alice = ClientFor(Alice);
        var (specimen, _, attachment) = await AttachedAsync(alice);

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content?inline=true", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("inline");
        response.Headers.GetValues("Content-Security-Policy").Single().ShouldStartWith("sandbox;");
    }

    [Fact]
    public async Task Download_WithInline_StillDownloadsATypeThatBrowsersWouldExecuteOrRender()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var text = await UploadOkAsync(alice, "说明.txt", "你好，这是一份说明"u8.ToArray());
        var added = await AddAttachmentAsync(alice, specimen, text, version);
        added.EnsureSuccessStatusCode();
        await WaitForAsync(text, "Attached");
        var attachment = await FirstAttachmentAsync(alice, specimen);

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content?inline=true", Ct);

        response.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment", "只有 PDF 和图片能直接打开");
    }

    [Fact]
    public async Task Download_WithARange_GivesPartialContent()
    {
        using var alice = ClientFor(Alice);
        var content = Pdf("0123456789");
        var (specimen, _, attachment) = await AttachedAsync(alice, content);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Root}/{specimen}/attachments/{attachment}/content");
        request.Headers.Range = new RangeHeaderValue(0, 4);

        var response = await alice.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.PartialContent);
        (await response.Content.ReadAsByteArrayAsync(Ct)).ShouldBe(content[..5]);
        response.Content.Headers.ContentRange!.ToString().ShouldBe($"bytes 0-4/{content.Length}");
    }

    [Fact]
    public async Task Download_ByAnotherViewerWhoSeesTheSpecimen_IsAllowed_AndByOneWhoDoesNot_Is404()
    {
        using var alice = ClientFor(Alice);
        var (specimen, _, attachment) = await AttachedAsync(alice);

        // 乙看得到全部数据；丙只看自己的
        using var bob = ClientFor(Bob);
        using var carol = ClientFor(Carol);
        (await bob.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var denied = await carol.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct);

        denied.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(denied)).GetProperty("code").GetString().ShouldBe("Sample.NotFound");
    }

    [Fact]
    public async Task Download_OfAnAttachmentThatBelongsToAnotherSpecimen_Is404()
    {
        using var alice = ClientFor(Alice);
        var (_, _, attachment) = await AttachedAsync(alice);
        var (other, _) = await CreateAsync(alice);

        var response = await alice.GetAsync($"{Root}/{other}/attachments/{attachment}/content", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Sample.AttachmentNotFound");
    }

    [Fact]
    public async Task Download_IsAudited()
    {
        using var alice = ClientFor(Alice);
        var (specimen, _, attachment) = await AttachedAsync(alice);
        await using var seed = NewSeed();
        var before = await seed.CountAsync("""SELECT count(*) FROM "Operations"."AuditLog" WHERE "Action" = 'Sample.GetSpecimenAttachmentContent'""");

        (await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct)).EnsureSuccessStatusCode();

        await Eventually.UntilAsync(
            _ => seed.CountAsync("""SELECT count(*) FROM "Operations"."AuditLog" WHERE "Action" = 'Sample.GetSpecimenAttachmentContent'"""),
            count => count > before,
            "下载留下审计记录，经发件箱汇总到 Operations",
            ct: Ct);
    }

    [Fact]
    public async Task Download_OfAPurgedFile_Is410()
    {
        using var alice = ClientFor(Alice);
        var (specimen, fileId, attachment) = await AttachedAsync(alice);
        await using var seed = NewSeed();
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Purged' WHERE "Id" = @id""", ("id", fileId));

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.Purged");
    }

    [Fact]
    public async Task Download_WhenTheObjectIsMissingFromTheStorage_IsAConflictNotAPermissionProblem()
    {
        using var alice = ClientFor(Alice);
        var (specimen, fileId, attachment) = await AttachedAsync(alice);
        await using var seed = NewSeed();
        File.Delete(StoragePathOf((await ColumnAsync<string>(seed, fileId, "StorageKey"))!));

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.FileContentMissing");
    }

    [Fact]
    public async Task DetachRequest_IsProcessed_TheFileIsDetachedWithAnExpiry_AndCanBeAttachedAgain()
    {
        using var alice = ClientFor(Alice);
        var (specimen, fileId, attachment) = await AttachedAsync(alice);
        var version = (await JsonAsync(await alice.GetAsync($"{Root}/{specimen}", Ct))).GetProperty("version").GetInt64();

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Root}/{specimen}/attachments/{attachment}") { Content = JsonContent.Create(new { expectedVersion = version }) };
        var removed = await alice.SendAsync(request, Ct);
        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent, await removed.Content.ReadAsStringAsync(Ct));

        await WaitForAsync(fileId, "Detached");
        await using var seed = NewSeed();
        (await IsNullAsync(seed, fileId, "ExpiresAt")).ShouldBeFalse("没有引用的文件保留一段时间后清理");
        (await seed.CountAsync($"""SELECT count(*) FROM "Files"."FileReference" WHERE "StoredFileId" = '{fileId}'""")).ShouldBe(0);

        // 保留期内可以重新关联，重新关联之后又是已关联、不再有到期时间
        var current = (await JsonAsync(await alice.GetAsync($"{Root}/{specimen}", Ct))).GetProperty("version").GetInt64();
        (await AddAttachmentAsync(alice, specimen, fileId, current)).StatusCode.ShouldBe(HttpStatusCode.OK);
        await WaitForAsync(fileId, "Attached");
        (await IsNullAsync(seed, fileId, "ExpiresAt")).ShouldBeTrue();
    }

    // 建一个标本、上传并关联一个 PDF，等 Worker 把文件处理成“已关联”
    private async Task<(Guid Specimen, Guid File, Guid Attachment)> AttachedAsync(HttpClient client, byte[]? content = null)
    {
        var (specimen, version) = await CreateAsync(client);
        var fileId = await UploadOkAsync(client, "合同.pdf", content ?? Pdf());
        (await AddAttachmentAsync(client, specimen, fileId, version)).EnsureSuccessStatusCode();
        await WaitForAsync(fileId, "Attached");
        return (specimen, fileId, await FirstAttachmentAsync(client, specimen));
    }

    private async Task WaitForAsync(Guid fileId, string status)
    {
        await using var seed = NewSeed();
        await Eventually.UntilAsync(
            _ => ColumnAsync<string>(seed, fileId, "Status"),
            current => string.Equals(current, status, StringComparison.Ordinal),
            $"Files 处理关联请求，文件变成 {status}",
            TimeSpan.FromSeconds(20),
            Ct);
    }

    private static async Task<Guid> FirstAttachmentAsync(HttpClient client, Guid specimen)
    {
        var detail = await JsonAsync(await client.GetAsync($"{Root}/{specimen}", Ct));
        return detail.GetProperty("attachments").EnumerateArray().First().GetProperty("id").GetGuid();
    }
}
