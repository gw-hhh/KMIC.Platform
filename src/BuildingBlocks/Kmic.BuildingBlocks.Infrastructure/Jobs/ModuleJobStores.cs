using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

internal sealed class ModuleJobStores(ModuleFrameworkTables tables, DataSources dataSources, JobExecutor executor, BackgroundJobCatalog catalog) : IModuleJobStores
{
    private readonly Lazy<FrozenDictionary<string, IModuleJobStore>> _byModule = new(() => tables.Modules
        .Order(StringComparer.Ordinal)
        .ToFrozenDictionary(m => m, m => (IModuleJobStore)new ModuleJobStore(m, tables, dataSources, executor, catalog), StringComparer.Ordinal));

    public IReadOnlyList<IModuleJobStore> All => [.. _byModule.Value.Values];

    public IModuleJobStore? Find(string module) => _byModule.Value.GetValueOrDefault(module);
}
