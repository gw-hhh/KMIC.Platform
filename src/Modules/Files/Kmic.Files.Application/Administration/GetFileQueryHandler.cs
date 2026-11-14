using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 处理 <see cref="GetFileQuery"/>
/// </summary>
public sealed class GetFileQueryHandler(IFilesQueryDb db, IStateHistoryQuery history) : IQueryHandler<GetFileQuery, FileDetail>
{
    /// <inheritdoc />
    public async ValueTask<Result<FileDetail>> Handle(GetFileQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var file = await db.Files.FirstOrDefaultAsync(f => f.Id == query.Id, cancellationToken);
        if (file is null)
        {
            return FilesErrors.NotFound;
        }

        var states = await history.GetAsync<StoredFile>(file.Id.Value, cancellationToken);
        return new FileDetail(
            file.Id,
            file.ConcurrencyVersion,
            file.OriginalName,
            file.FileType,
            file.ContentType,
            file.Size,
            file.Sha256,
            file.Profile,
            file.UploadedBy,
            file.UploadedAt,
            file.Status,
            file.ExpiresAt,
            file.RejectReason,
            file.PurgedAt,
            file.PurgedBy,
            file.PurgeReason,
            file.ObjectDeletedAt,
            file.HistoryPurgedAt,
            file.HistoryPurgeNote,
            [.. file.References.Select(r => new FileReferenceItem(r.OwnerModule, r.OwnerType, r.OwnerId, r.AttachedAt))],
            states);
    }
}
