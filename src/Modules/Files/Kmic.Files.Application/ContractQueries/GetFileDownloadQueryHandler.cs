using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 查询文件下载信息
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
            .Select(f => new { f.Id, f.StorageKey, f.OriginalName, f.FileType, f.ContentType, f.Size, f.Sha256, f.Status, f.Profile })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return FilesErrors.NotFound;
        }

        return ErrorOf(row.Status, row.Profile) is { } refused
            ? refused
            : AsDownload(row.Id.Value, row.StorageKey, row.OriginalName, row.FileType, row.ContentType, row.Size, row.Sha256);
    }

    // 导出等生成文件可能没有业务关联，登记为可用后也允许下载
    private static Error? ErrorOf(StoredFileStatus status, string profile)
    {
        if (status == StoredFileStatus.Attached
            || (status == StoredFileStatus.Available && profile.StartsWith(StoredFile.GeneratedProfilePrefix, StringComparison.Ordinal)))
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

    private static Result<FileDownload> AsDownload(Guid id, string storageKey, string name, string? fileType, string? contentType, long? size, string? sha256) =>
        fileType is null || contentType is null || size is null || sha256 is null
            ? FilesErrors.NotReady
            : new FileDownload(id, storageKey, name, contentType, size.Value, sha256, fileType);
}
