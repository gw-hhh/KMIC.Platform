using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence.Repositories;

internal sealed class StorageIssueRepository(FilesDbContext context)
    : EfRepository<FilesDbContext, StorageIssue, StorageIssueId>(context), IStorageIssueRepository
{
    protected override Error NotFound => FilesErrors.StorageIssueNotFound;

    public async Task<IReadOnlySet<StoredFileId>> FindOpenFileIdsAsync(IReadOnlyCollection<StoredFileId> fileIds, CancellationToken ct) =>
        (await Context.Set<StorageIssue>()
            .Where(i => i.Status == StorageIssueStatus.Open && fileIds.Contains(i.FileId))
            .Select(i => i.FileId)
            .ToListAsync(ct)).ToHashSet();
}
