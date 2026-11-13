using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="CheckAttachabilityQuery"/>
/// </summary>
public sealed class CheckAttachabilityQueryHandler(IFilesQueryDb db) : IQueryHandler<CheckAttachabilityQuery, IReadOnlyList<FileSnapshot>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<FileSnapshot>>> Handle(CheckAttachabilityQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ids = query.FileIds.Select(StoredFileId.From).ToList();
        var uploader = UserId.From(query.UploaderId);
        var bySystem = uploader == UserId.System;
        var rows = await db.Files
            .Where(f => ids.Contains(f.Id)
                && (f.Status == StoredFileStatus.Detached
                    || (f.Status == StoredFileStatus.Available && (f.UploadedBy == uploader || (bySystem && f.UploadedBy == null)))))
            .Select(f => new { f.Id, f.OriginalName, f.Size, f.FileType })
            .ToListAsync(cancellationToken);
        var snapshots = rows
            .Where(r => r.Size is not null && r.FileType is not null)
            .Select(r => new FileSnapshot(r.Id.Value, r.OriginalName, r.Size!.Value, r.FileType!))
            .ToList();
        return snapshots;
    }
}
