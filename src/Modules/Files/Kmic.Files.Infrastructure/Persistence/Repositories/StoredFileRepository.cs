using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence.Repositories;

internal sealed class StoredFileRepository(FilesDbContext context)
    : EfRepository<FilesDbContext, StoredFile, StoredFileId>(context), IStoredFileRepository
{
    protected override Error NotFound => FilesErrors.NotFound;

    public async Task<bool> ExistsAsync(StoredFileId id, CancellationToken ct) =>
        await Context.Set<StoredFile>().AnyAsync(f => f.Id == id, ct);
}
