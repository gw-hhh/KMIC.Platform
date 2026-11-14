using System.Security.Cryptography;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Files.Application;
using Kmic.Files.Application.Attachments;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Errors;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 模块生成的文件：写入存储并请求登记，登记之后系统能读取内容
/// </summary>
/// <remarks>
/// 发请求的发布器换成记录用的，其余（存储、类型识别、登记处理）都是真的：先看请求里写了什么，再把它交给登记命令
/// </remarks>
public sealed class GeneratedFileTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template, storageRoot: _root), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Source = "Test.Export";

    private static readonly string _root = Path.Combine(Path.GetTempPath(), "kmic-generated-tests-" + Guid.CreateVersion7().ToString("N"));

    private async Task<(Result<GeneratedFile> Result, List<IIntegrationRequest> Sent)> WriteAsync(string fileName, byte[] content, TimeSpan? retention = null)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();
        var services = scope.ServiceProvider;
        var sent = new List<IIntegrationRequest>();
        var generated = new GeneratedFiles(
            services.GetRequiredService<IFileUploadProcessor>(),
            services.GetRequiredService<IStorageKeyFactory>(),
            new RecordingPublisher(sent),
            services.GetRequiredService<IExecutionContext>(),
            services.GetRequiredService<IOptions<FilesOptions>>(),
            TimeProvider.System);
        await using var stream = new MemoryStream(content);
        var written = await generated.WriteAsync(Source, fileName, stream, retention ?? TimeSpan.FromDays(7), Ct);

        // 登记是第二步：成功写入之后，由调用方在命令里做
        if (written.IsSuccess)
        {
            generated.Register(written.Value);
        }

        return (written, sent);
    }

    private async Task RegisterAsync(RegisterGeneratedFile message)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();
        (await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new RegisterGeneratedFileCommand(message), Ct)).ShouldBeSuccess();
    }

    private async Task<Result<FileContent>> OpenAsync(Guid fileId)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();
        return await scope.ServiceProvider.GetRequiredService<IFileContents>().OpenReadAsync(fileId, Ct);
    }

    private static async Task<byte[]> ReadAllAsync(FileContent content)
    {
        await using var stream = content.Content;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, Ct);
        return buffer.ToArray();
    }

    [Fact]
    public async Task Write_PutsTheObjectInStorage_AndRequestsRegistrationWithWhatWasWritten()
    {
        var bytes = Pdf("导出的内容");

        var (result, sent) = await WriteAsync("../../导出.pdf", bytes, TimeSpan.FromDays(3));

        var file = result.ShouldBeSuccess();
        file.Name.ShouldBe("导出.pdf", "路径部分被去掉");
        file.FileType.ShouldBe("pdf");
        file.Size.ShouldBe(bytes.Length);
        var message = sent.ShouldHaveSingleItem().ShouldBeOfType<RegisterGeneratedFile>();
        message.FileId.ShouldBe(file.FileId);
        message.Source.ShouldBe(Source);
        message.Sha256.ShouldBe(Convert.ToHexStringLower(SHA256.HashData(bytes)));
        message.ContentType.ShouldBe("application/pdf");
        message.ExpiresAt.ShouldBe(TimeProvider.System.GetUtcNow().AddDays(3), TimeSpan.FromMinutes(1));
        (await File.ReadAllBytesAsync(Path.Combine(_root, message.StorageKey.Replace('/', Path.DirectorySeparatorChar)), Ct)).ShouldBe(bytes);
    }

    [Fact]
    public async Task Registered_BecomesAnAvailableFileWithTheGeneratedProfile_AndRegisteringAgainChangesNothing()
    {
        await using var seed = NewSeed();
        var (result, sent) = await WriteAsync("报表.pdf", Pdf("报表"));
        var message = (RegisterGeneratedFile)sent.Single();

        await RegisterAsync(message);
        await RegisterAsync(message);

        var id = result.ShouldBeSuccess().FileId;
        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available");
        (await ColumnAsync<string>(seed, id, "Profile")).ShouldBe("Generated:" + Source);
        (await seed.CountAsync($"""SELECT count(*) FROM "Files"."StoredFile" WHERE "Id" = '{id}'""")).ShouldBe(1);
    }

    [Fact]
    public async Task Write_RefusesEmptyContentAndUnknownTypes_WithoutRequestingRegistration()
    {
        var (empty, sentForEmpty) = await WriteAsync("空的.pdf", []);
        var (unknown, sentForUnknown) = await WriteAsync("奇怪.xyz", [1, 2, 3]);

        empty.ShouldBeFailure(FilesErrors.EmptyFile);
        unknown.ShouldBeFailure(FilesErrors.TypeNotAllowed);
        sentForEmpty.ShouldBeEmpty();
        sentForUnknown.ShouldBeEmpty();
    }

    [Fact]
    public async Task SystemCanReadARegisteredFile_ButNotOneThatIsMissingPurgedOrNotYetRegistered()
    {
        await using var seed = NewSeed();
        var bytes = Pdf("附件内容");
        var (result, sent) = await WriteAsync("附件.pdf", bytes);
        var id = result.ShouldBeSuccess().FileId;

        (await OpenAsync(id)).ShouldBeFailure(FilesErrors.NotFound);
        await RegisterAsync((RegisterGeneratedFile)sent.Single());

        var content = (await OpenAsync(id)).ShouldBeSuccess();
        content.Name.ShouldBe("附件.pdf");
        content.ContentType.ShouldBe("application/pdf");
        (await ReadAllAsync(content)).ShouldBe(bytes);

        var key = (await ColumnAsync<string>(seed, id, "StorageKey"))!;
        File.Delete(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        (await OpenAsync(id)).ShouldBeFailure(Kmic.BuildingBlocks.Domain.Errors.CommonErrors.FileContentMissing);

        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Purged' WHERE "Id" = @id""", ("id", id));
        (await OpenAsync(id)).ShouldBeFailure(FilesErrors.Purged);
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Uploading' WHERE "Id" = @id""", ("id", id));
        (await OpenAsync(id)).ShouldBeFailure(FilesErrors.NotReady);
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Rejected' WHERE "Id" = @id""", ("id", id));
        (await OpenAsync(id)).ShouldBeFailure(FilesErrors.NotFound);
    }

    [Fact]
    public async Task ARegisteredGeneratedFile_CanBeDownloaded_WhileAnOrdinaryTemporaryFileCannotUntilItIsAttached()
    {
        var (result, sent) = await WriteAsync("导出结果.pdf", Pdf("结果"));
        await RegisterAsync((RegisterGeneratedFile)sent.Single());
        using var alice = ClientFor(Alice);
        var ordinary = await UploadOkAsync(alice, "普通上传.pdf", Pdf());
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(CallOrigin.ModuleCall, CurrentUser.From(Alice), Alice);
        var downloads = scope.ServiceProvider.GetRequiredService<IFileDownloads>();

        var generated = await downloads.GetAsync(result.ShouldBeSuccess().FileId, Ct);
        var temporary = await downloads.GetAsync(ordinary, Ct);

        generated.ShouldBeSuccess().Name.ShouldBe("导出结果.pdf");
        temporary.ShouldBeFailure(FilesErrors.NotReady);
    }

    [Fact]
    public async Task OrdinaryUsersCannotReadFileContentsThroughTheContract()
    {
        var (result, sent) = await WriteAsync("别人的.pdf", Pdf());
        await RegisterAsync((RegisterGeneratedFile)sent.Single());
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(CallOrigin.ModuleCall, CurrentUser.From(Alice), Alice);

        var opened = await scope.ServiceProvider.GetRequiredService<IFileContents>().OpenReadAsync(result.ShouldBeSuccess().FileId, Ct);

        opened.IsFailure.ShouldBeTrue();
        opened.Error.Type.ShouldBe(ErrorType.Forbidden);
    }

    private sealed class RecordingPublisher(List<IIntegrationRequest> sent) : IIntegrationEventPublisher
    {
        public void Add(IIntegrationEvent integrationEvent) => throw new NotSupportedException();

        public void Send(IIntegrationRequest request) => sent.Add(request);
    }
}
