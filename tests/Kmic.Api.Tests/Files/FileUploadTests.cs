using System.Net;
using System.Security.Cryptography;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 上传端点：登记、流式写存储、检查类型与大小、登记结果，每一步的失败都有对应的状态码和文件状态
/// </summary>
public sealed class FileUploadTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const long Limit = 10 * 1024 * 1024;

    [Fact]
    public async Task Upload_AValidPdf_IsStoredAndRegisteredAsAvailable()
    {
        using var alice = ClientFor(Alice);
        var content = Pdf();

        var response = await UploadAsync(alice, "合同.pdf", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await JsonAsync(response);
        body.GetProperty("name").GetString().ShouldBe("合同.pdf");
        body.GetProperty("size").GetInt64().ShouldBe(content.Length);
        body.GetProperty("type").GetString().ShouldBe("pdf");
        var now = TimeProvider.System.GetUtcNow();
        body.GetProperty("expiresAt").GetDateTimeOffset().ShouldBeInRange(now.AddHours(23), now.AddHours(25));

        await using var seed = NewSeed();
        var fileId = body.GetProperty("fileId").GetGuid();
        (await ColumnAsync<string>(seed, fileId, "Status")).ShouldBe("Available");
        (await ColumnAsync<string>(seed, fileId, "Sha256")).ShouldBe(Convert.ToHexStringLower(SHA256.HashData(content)));
        (await ColumnAsync<Guid>(seed, fileId, "UploadedBy")).ShouldBe(Alice.UserId.Value);
        var key = (await ColumnAsync<string>(seed, fileId, "StorageKey"))!;
        key.ShouldNotContain("合同", customMessage: "存储键由文件 ID 生成，和文件名无关");
        (await File.ReadAllBytesAsync(StoragePathOf(key), Ct)).ShouldBe(content);
    }

    [Fact]
    public async Task Upload_AFileNameWithAPath_IsStoredWithoutIt()
    {
        using var alice = ClientFor(Alice);

        var response = await UploadAsync(alice, @"C:\Users\someone\合同.pdf", Pdf());

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await JsonAsync(response)).GetProperty("name").GetString().ShouldBe("合同.pdf");
    }

    [Fact]
    public async Task Upload_ANonPdfNamedAsPdf_IsRefusedWith415_AndTheRowIsRejected()
    {
        using var alice = ClientFor(Alice);
        await using var seed = NewSeed();
        var before = await RowsAsync(seed, "Rejected");

        var response = await UploadAsync(alice, "假的.pdf", "MZ......这不是 PDF"u8.ToArray());

        response.StatusCode.ShouldBe((HttpStatusCode)415);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.TypeNotAllowed");
        (await RowsAsync(seed, "Rejected")).ShouldBe(before + 1);
    }

    [Fact]
    public async Task Upload_MoreThanTheProfileAllows_IsRefusedWith413_AndNothingStaysInTheStorage()
    {
        using var alice = ClientFor(Alice);
        await using var seed = NewSeed();
        var before = await RowsAsync(seed, "Rejected");
        var oversized = new byte[Limit + 1];
        "%PDF-1.7\n"u8.CopyTo(oversized);

        var response = await UploadAsync(alice, "太大.pdf", oversized);

        response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.TooLarge");
        (await RowsAsync(seed, "Rejected")).ShouldBe(before + 1);
        var key = await seed.ScalarAsync<string>("""SELECT "StorageKey" FROM "Files"."StoredFile" WHERE "OriginalName" = '太大.pdf' ORDER BY "UploadedAt" DESC LIMIT 1""");
        File.Exists(StoragePathOf(key!)).ShouldBeFalse("超过上限时写到一半的内容由存储自己清理");
    }

    [Fact]
    public async Task Upload_ADeclaredSizeOverTheLimit_IsRefusedBeforeAnythingIsRead()
    {
        using var alice = ClientFor(Alice);
        await using var seed = NewSeed();
        var before = await seed.CountAsync("""SELECT count(*) FROM "Files"."StoredFile" """);

        var response = await UploadAsync(alice, "声明太大.pdf", Pdf(), declaredSize: Limit * 2);

        response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
        (await seed.CountAsync("""SELECT count(*) FROM "Files"."StoredFile" """)).ShouldBe(before, "声明就超限的，连登记行都不建");
    }

    [Fact]
    public async Task Upload_AnEmptyFile_IsRefusedWith400()
    {
        using var alice = ClientFor(Alice);

        var response = await UploadAsync(alice, "空.pdf", []);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.EmptyFile");
    }

    [Fact]
    public async Task Upload_WithoutThePermissionOfTheProfile_IsForbidden()
    {
        // 丙有查看和创建，没有“编辑”，而这个档案要的是编辑
        using var carol = ClientFor(Carol);

        var response = await UploadAsync(carol, "合同.pdf", Pdf());

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.MissingPermission");
    }

    [Fact]
    public async Task Upload_AnUnknownProfile_IsRefusedWith400()
    {
        using var alice = ClientFor(Alice);

        var response = await UploadAsync(alice, "合同.pdf", Pdf(), profile: "Nobody.Declared");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Files.ProfileNotFound");
    }

    [Fact]
    public async Task Upload_WithoutLoggingIn_IsRefusedWith401()
    {
        using var anonymous = ClientFor();

        var response = await UploadAsync(anonymous, "合同.pdf", Pdf());

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<long> RowsAsync(IdentitySeed seed, string status) =>
        await seed.CountAsync($"""SELECT count(*) FROM "Files"."StoredFile" WHERE "Status" = '{status}'""");
}
