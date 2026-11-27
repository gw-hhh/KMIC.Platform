using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.ContractQueries;

public sealed class GetReadableFileQueryHandler(IFilesQueryDb db) : IQueryHandler<GetReadableFileQuery, FileDownload>
{
    public async ValueTask<Result<FileDownload>> Handle(GetReadableFileQuery query, CancellationToken cancellationToken)
    {
        var id = StoredFileId.From(query.FileId);
        var row = await db.Files
            .Where(f => f.Id == id)
            .Select(f => new { f.Id, f.StorageKey, f.OriginalName, f.FileType, f.ContentType, f.Size, f.Sha256, f.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return FilesErrors.NotFound;
        }

        if (ErrorOf(row.Status) is { } refused)
        {
            return refused;
        }

        return row.FileType is null || row.ContentType is null || row.Size is null || row.Sha256 is null
            ? FilesErrors.NotReady
            : new FileDownload(row.Id.Value, row.StorageKey, row.OriginalName, row.ContentType, row.Size.Value, row.Sha256, row.FileType);
    }

    private static Error? ErrorOf(StoredFileStatus status)
    {
        if (status == StoredFileStatus.Attached || status == StoredFileStatus.Available || status == StoredFileStatus.Detached)
        {
            return null;
        }

        if (status == StoredFileStatus.Purged)
        {
            return FilesErrors.Purged;
        }

        return status == StoredFileStatus.Uploading || status == StoredFileStatus.Scanning ? FilesErrors.NotReady : FilesErrors.NotFound;
    }
}
