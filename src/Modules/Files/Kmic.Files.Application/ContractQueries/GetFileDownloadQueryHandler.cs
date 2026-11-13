using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetFileDownloadQuery"/>
/// </summary>
public sealed class GetFileDownloadQueryHandler(IFilesQueryDb db) : IQueryHandler<GetFileDownloadQuery, FileDownload>
{
    /// <inheritdoc />
    public async ValueTask<Result<FileDownload>> Handle(GetFileDownloadQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var id = StoredFileId.From(query.FileId);
        var row = await db.Files
            .Where(f => f.Id == id)
            .Select(f => new { f.Id, f.StorageKey, f.OriginalName, f.FileType, f.ContentType, f.Size, f.Sha256, f.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return FilesErrors.NotFound;
        }

        return ErrorOf(row.Status) is { } refused
            ? refused
            : AsDownload(row.Id.Value, row.StorageKey, row.OriginalName, row.FileType, row.ContentType, row.Size, row.Sha256);
    }

    // 只有已关联的文件能下载；彻底删除的告诉调用方已删除，还没登记完的可以稍后再试，其余（已解除关联、被拒绝、清理中）当作不存在
    private static Error? ErrorOf(StoredFileStatus status)
    {
        if (status == StoredFileStatus.Attached)
        {
            return null;
        }

        if (status == StoredFileStatus.Purged)
        {
            return FilesErrors.Purged;
        }

        return status == StoredFileStatus.Uploading || status == StoredFileStatus.Scanning || status == StoredFileStatus.Available
            ? FilesErrors.NotReady
            : FilesErrors.NotFound;
    }

    // 已关联的文件一定写完了，类型、大小、摘要都有；缺哪个都是数据损坏，当作还没准备好
    private static Result<FileDownload> AsDownload(Guid id, string storageKey, string name, string? fileType, string? contentType, long? size, string? sha256) =>
        fileType is null || contentType is null || size is null || sha256 is null
            ? FilesErrors.NotReady
            : new FileDownload(id, storageKey, name, contentType, size.Value, sha256, fileType);
}
