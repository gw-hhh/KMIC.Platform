using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// <see cref="IModuleJobStores" /> 的实现：每个注册了数据库的模块一个存储，用到时才建
/// </summary>
/// <remarks>
/// 模块名单来自模块的写入上下文登记，要等模型加载，所以不在构造函数里取
/// </remarks>
internal sealed class ModuleJobStores(ModuleFrameworkTables tables, DataSources dataSources, JobExecutor executor) : IModuleJobStores
{
    private readonly Lazy<FrozenDictionary<string, IModuleJobStore>> _byModule = new(() => tables.Modules
        .Order(StringComparer.Ordinal)
        .ToFrozenDictionary(m => m, m => (IModuleJobStore)new ModuleJobStore(m, tables, dataSources, executor), StringComparer.Ordinal));

    public IReadOnlyList<IModuleJobStore> All => [.. _byModule.Value.Values];

    public IModuleJobStore? Find(string module) => _byModule.Value.GetValueOrDefault(module);
}
