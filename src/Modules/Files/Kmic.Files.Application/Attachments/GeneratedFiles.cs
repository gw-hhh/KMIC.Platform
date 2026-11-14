using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Application.Uploads;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// <see cref="IGeneratedFiles"/> 的实现：用上传同样的流程写入存储，再请求登记
/// </summary>
public sealed class GeneratedFiles(
    IFileUploadProcessor processor,
    IStorageKeyFactory keys,
    IIntegrationEventPublisher events,
    IExecutionContext execution,
    IOptions<FilesOptions> options,
    TimeProvider time) : IGeneratedFiles
{
    /// <inheritdoc />
    public async Task<Result<GeneratedFile>> WriteAsync(string source, string fileName, Stream content, TimeSpan retention, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);

        if (FileNames.Sanitize(fileName) is not { } name)
        {
            throw new ArgumentException("文件名不能为空。", nameof(fileName));
        }

        var now = time.GetUtcNow();
        var id = Guid.CreateVersion7();
        var key = keys.KeyFor(id, now);

        // 生成的文件不按上传档案限制类型，只限制大小
        var profile = new UploadProfile(
            StoredFile.GeneratedProfilePrefix + source,
            "生成的文件",
            FilesModuleInfo.Name,
            options.Value.GeneratedFileMaxMb * 1024L * 1024L,
            [.. FileTypes.All.Select(t => t.Code)],
            string.Empty);
        var stored = await processor.StoreAsync(profile, name, content, key, ct);
        if (stored.File is not { } file)
        {
            return stored.Outcome switch
            {
                UploadOutcome.TooLarge => FilesErrors.TooLarge,
                UploadOutcome.Empty => FilesErrors.EmptyFile,
                _ => FilesErrors.TypeNotAllowed,
            };
        }

        return new GeneratedFile(
            id,
            file.Key,
            file.VersionId,
            name,
            file.Type.Code,
            file.Type.ContentType,
            file.Size,
            file.Sha256,
            source,
            execution.User.UserId?.Value,
            now + retention);
    }

    /// <inheritdoc />
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "这里是唯一发出登记请求的地方")]
    public void Register(GeneratedFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        events.Send(new RegisterGeneratedFile(
            file.FileId,
            file.StorageKey,
            file.ObjectVersionId,
            file.Name,
            file.FileType,
            file.ContentType,
            file.Size,
            file.Sha256,
            file.Source,
            file.RequestedBy,
            file.ExpiresAt));
    }
}
