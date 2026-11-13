using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 处理 <see cref="ListFilesQuery"/>
/// </summary>
public sealed class ListFilesQueryHandler(IFilesQueryDb db) : IQueryHandler<ListFilesQuery, PagedResult<FileListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<FileListItem>>> Handle(ListFilesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var statuses = EnumerationFilter.Parse<StoredFileStatus>(query.Status, "status");
        if (statuses.IsFailure)
        {
            return statuses.Error;
        }

        var files = db.Files.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            files = files.Where(f => KmicFunctions.ContainsText(f.OriginalName, query.Q));
        }

        var wanted = statuses.Value;
        if (wanted.Count > 0)
        {
            files = files.Where(f => wanted.Contains(f.Status));
        }

        if (!string.IsNullOrWhiteSpace(query.Profile))
        {
            files = files.Where(f => f.Profile == query.Profile);
        }

        if (query.UploadedBy is { } uploadedBy)
        {
            files = files.Where(f => f.UploadedBy == uploadedBy);
        }

        if (query.From is { } from)
        {
            files = files.Where(f => f.UploadedAt >= from);
        }

        if (query.To is { } to)
        {
            files = files.Where(f => f.UploadedAt < to);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerModule))
        {
            files = files.Where(f => f.References.Any(r => r.OwnerModule == query.OwnerModule
                && (query.OwnerType == null || r.OwnerType == query.OwnerType)
                && (query.OwnerId == null || r.OwnerId == query.OwnerId)));
        }

        return await files
            .Select(f => new FileListItem(f.Id, f.OriginalName, f.FileType, f.Size, f.Profile, f.UploadedBy, f.UploadedAt, f.Status, f.ExpiresAt, f.ConcurrencyVersion))
            .ToPagedAsync(query.Page, query.Sort, FileSort.Map, cancellationToken);
    }
}
