using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Kmic.Files.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence.Repositories;

internal sealed class StoredFileRepository(FilesDbContext context)
    : EfRepository<FilesDbContext, StoredFile, StoredFileId>(context), IStoredFileRepository
{
    protected override Error NotFound => FilesErrors.NotFound;

    public async Task<bool> ExistsAsync(StoredFileId id, CancellationToken ct) =>
        await Context.Set<StoredFile>().AnyAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<StoredFileId>> FindExpiredAsync(StoredFileStatus status, DateTimeOffset now, int limit, CancellationToken ct) =>
        await Context.Set<StoredFile>()
            .Where(f => f.Status == status && f.ExpiresAt <= now)
            .OrderBy(f => f.ExpiresAt)
            .Select(f => f.Id)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StoredFileId>> FindStaleUploadsAsync(DateTimeOffset startedBefore, int limit, CancellationToken ct) =>
        await Context.Set<StoredFile>()
            .Where(f => f.Status == StoredFileStatus.Uploading && f.UploadedAt <= startedBefore)
            .OrderBy(f => f.UploadedAt)
            .Select(f => f.Id)
            .Take(limit)
            .ToListAsync(ct);

    // 在删除队列里的：删除中，或已彻底删除而对象还没删。只改领取与重试的列，不经聚合
    public async Task<bool> ResetDeletionAsync(StoredFileId id, CancellationToken ct) =>
        await UpdateWhereAsync(
            f => f.Id == id && (f.Status == StoredFileStatus.Deleting || (f.Status == StoredFileStatus.Purged && f.ObjectDeletedAt == null)),
            setters => setters
                .SetProperty(f => EF.Property<int>(f, StoredFileConfiguration.DeleteAttempts), 0)
                .SetProperty(f => EF.Property<DateTimeOffset?>(f, StoredFileConfiguration.NextAttemptAt), (DateTimeOffset?)null)
                .SetProperty(f => EF.Property<Guid?>(f, StoredFileConfiguration.LeaseToken), (Guid?)null)
                .SetProperty(f => EF.Property<DateTimeOffset?>(f, StoredFileConfiguration.LeaseUntil), (DateTimeOffset?)null),
            ct) > 0;
}
