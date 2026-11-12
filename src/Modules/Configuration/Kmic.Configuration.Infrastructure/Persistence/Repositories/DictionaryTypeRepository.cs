using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence.Repositories;

internal sealed class DictionaryTypeRepository(ConfigurationDbContext context)
    : EfRepository<ConfigurationDbContext, DictionaryType, DictionaryTypeId>(context), IDictionaryTypeRepository
{
    // 咨询锁的键只要在整个库里不和别的用途撞上
    private const long CatalogSyncLockKey = 0x4B4D_4943_0002;

    protected override Error NotFound => ConfigurationErrors.DictionaryTypeNotFound;

    // 字典项总是一起加载
    protected override IQueryable<DictionaryType> Aggregate(IQueryable<DictionaryType> set) => set.Include(t => t.Items);

    public async Task<Result<DictionaryType>> GetByCodeForUpdateAsync(string typeCode, long? expectedVersion, CancellationToken ct)
    {
        var id = await Context.Set<DictionaryType>()
            .Where(t => t.Code == typeCode)
            .Select(t => (DictionaryTypeId?)t.Id)
            .SingleOrDefaultAsync(ct);
        return id is null ? ConfigurationErrors.DictionaryTypeNotFound : await GetForUpdateAsync(id.Value, expectedVersion, ct);
    }

    public async Task<IReadOnlyList<DictionaryType>> ListWithoutItemsAsync(CancellationToken ct) =>
        await Context.Set<DictionaryType>().ToListAsync(ct);

    public async Task LockCatalogSyncAsync(CancellationToken ct) =>
        await Context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({CatalogSyncLockKey})", ct);
}
