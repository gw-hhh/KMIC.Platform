using System.Net;
using System.Net.Http.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 业务模块怎样用文件：上传得到文件 ID，在业务命令里关联，关联与业务写入在同一个事务里
/// </summary>
/// <remarks>
/// 这里只有 Api 角色，关联请求停在发件箱里，Files 一侧还是临时状态；请求被处理之后的变化见 FileLifecycleTests
/// </remarks>
public sealed class SpecimenAttachmentTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimens";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东")));

    [Fact]
    public async Task AddAttachment_AFreshUpload_ShowsUpInTheDetail_WhileTheFileStaysTemporaryInFiles()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());

        var added = await AddAttachmentAsync(alice, specimen, fileId, version);

        added.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await JsonAsync(await alice.GetAsync($"{Root}/{specimen}", Ct));
        var attachment = detail.GetProperty("attachments").EnumerateArray().Single();
        attachment.GetProperty("name").GetString().ShouldBe("合同.pdf");
        attachment.GetProperty("size").GetInt64().ShouldBe(Pdf().Length);
        attachment.GetProperty("fileType").GetString().ShouldBe("pdf");
        detail.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()).ShouldContain("Edit", StringComparer.Ordinal);
        await using var seed = NewSeed();
        (await ColumnAsync<string>(seed, fileId, "Status")).ShouldBe("Available", "关联请求要由 Worker 处理，这里没有");
    }

    [Fact]
    public async Task AddAttachment_TheSameFileTwice_IsRefused()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());
        var first = await AddAttachmentAsync(alice, specimen, fileId, version);
        var next = (await JsonAsync(first)).GetProperty("version").GetInt64();

        var second = await AddAttachmentAsync(alice, specimen, fileId, next);

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(second)).GetProperty("code").GetString().ShouldBe("Sample.AttachmentExists");
    }

    [Fact]
    public async Task AddAttachment_AFileSomeoneElseUploaded_IsRefusedAsNotAttachable()
    {
        var dave = Dave();
        using var alice = ClientFor(Alice);
        using var other = ClientFor(dave);
        var (specimen, version) = await CreateAsync(alice);
        var theirs = await UploadOkAsync(other, "别人的.pdf", Pdf());

        var response = await AddAttachmentAsync(alice, specimen, theirs, version);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.NotAttachable");
        var detail = await JsonAsync(await alice.GetAsync($"{Root}/{specimen}", Ct));
        detail.GetProperty("attachments").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task AddAttachment_AFileThatDoesNotExist_IsRefusedAsNotAttachable()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);

        var response = await AddAttachmentAsync(alice, specimen, Guid.CreateVersion7(), version);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.NotAttachable");
    }

    [Fact]
    public async Task AddAttachment_ByAnEditorWhoIsNotTheOwner_IsForbidden()
    {
        var dave = Dave();
        using var alice = ClientFor(Alice);
        using var other = ClientFor(dave);
        var (specimen, version) = await CreateAsync(alice);
        var theirs = await UploadOkAsync(other, "别人的.pdf", Pdf());

        var response = await AddAttachmentAsync(other, specimen, theirs, version);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Sample.NotOwner");
    }

    [Fact]
    public async Task AddAttachment_WithoutTheEditPermission_IsForbidden()
    {
        using var alice = ClientFor(Alice);
        using var bob = ClientFor(Bob);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());

        var response = await AddAttachmentAsync(bob, specimen, fileId, version);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.MissingPermission");
    }

    [Fact]
    public async Task AddAttachment_ToAClosedSpecimen_IsRefused()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var submitted = await PostActionAsync(alice, $"{Root}/{specimen}/submit", new { expectedVersion = version });
        var closed = await PostActionAsync(alice, $"{Root}/{specimen}/close", new { expectedVersion = (await JsonAsync(submitted)).GetProperty("version").GetInt64() });
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());

        var response = await AddAttachmentAsync(alice, specimen, fileId, (await JsonAsync(closed)).GetProperty("version").GetInt64());

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Sample.InvalidState");
    }

    [Fact]
    public async Task Download_BeforeFilesHasRegisteredTheReference_AsksTheCallerToTryAgain()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());
        await AddAttachmentAsync(alice, specimen, fileId, version);
        var attachment = await FirstAttachmentAsync(alice, specimen);

        var response = await alice.GetAsync($"{Root}/{specimen}/attachments/{attachment}/content", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.NotReady");
    }

    [Fact]
    public async Task RemoveAttachment_TakesItOutOfTheDetail()
    {
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice);
        var fileId = await UploadOkAsync(alice, "合同.pdf", Pdf());
        var added = await AddAttachmentAsync(alice, specimen, fileId, version);
        var attachment = await FirstAttachmentAsync(alice, specimen);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Root}/{specimen}/attachments/{attachment}")
        {
            Content = JsonContent.Create(new { expectedVersion = (await JsonAsync(added)).GetProperty("version").GetInt64() }),
        };
        var removed = await alice.SendAsync(request, Ct);

        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent, await removed.Content.ReadAsStringAsync(Ct));
        var detail = await JsonAsync(await alice.GetAsync($"{Root}/{specimen}", Ct));
        detail.GetProperty("attachments").GetArrayLength().ShouldBe(0);
    }

    private static async Task<Guid> FirstAttachmentAsync(HttpClient client, Guid specimen)
    {
        var detail = await JsonAsync(await client.GetAsync($"{Root}/{specimen}", Ct));
        return detail.GetProperty("attachments").EnumerateArray().First().GetProperty("id").GetGuid();
    }

    // 有“编辑”权限、能看全部数据的另一个人：能上传、能看到别人的标本，但不是负责人
    private TestIdentity Dave()
    {
        var dave = TestIdentity.User("丁").InTree(_org).InOrgUnit(_org["华东"]).WithRole(r => r
            .Permissions(SamplePermissions.View, SamplePermissions.Edit)
            .Scope(ScopeLevel.All));
        Factory.Loader.Register(dave);
        return dave;
    }
}
