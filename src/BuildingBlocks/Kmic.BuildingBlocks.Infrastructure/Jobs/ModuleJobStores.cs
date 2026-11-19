using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// <see cref="IModuleJobStores" /> 的实现：每个注册了数据库的模块一个存储，用到时才建
/// </summary>
internal sealed class ModuleJobStores(ModuleFrameworkTables tables, DataSources dataSources, JobExecutor executor, BackgroundJobCatalog catalog) : IModuleJobStores
{
    private readonly Lazy<FrozenDictionary<string, IModuleJobStore>> _byModule = new(() => tables.Modules
        .Order(StringComparer.Ordinal)
        .ToFrozenDictionary(m => m, m => (IModuleJobStore)new ModuleJobStore(m, tables, dataSources, executor, catalog), StringComparer.Ordinal));

    public IReadOnlyList<IModuleJobStore> All => [.. _byModule.Value.Values];

    public IModuleJobStore? Find(string module) => _byModule.Value.GetValueOrDefault(module);
}
