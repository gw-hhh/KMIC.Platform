using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed class ModuleMessageStores(ModuleFrameworkTables tables, DataSources dataSources, SubscriptionCatalog catalog) : IModuleMessageStores
{
    private readonly Lazy<FrozenDictionary<string, IModuleMessageStore>> _byModule = new(() => tables.Modules
        .Order(StringComparer.Ordinal)
        .ToFrozenDictionary(m => m, m => (IModuleMessageStore)new ModuleMessageStore(m, tables, dataSources, catalog), StringComparer.Ordinal));

    public IReadOnlyList<IModuleMessageStore> All => [.. _byModule.Value.Values];

    public IModuleMessageStore? Find(string module) => _byModule.Value.GetValueOrDefault(module);

    public async Task<IReadOnlySet<string>> GetRetiringSubscriptionsAsync(CancellationToken ct)
    {
        await using var command = dataSources.Reads(CallOrigin.Http).CreateCommand(
            $"""SELECT DISTINCT "Subscription" FROM {MessageRouteTable.Name} WHERE "Status" = {MessageRouteTable.Retiring}""");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var retiring = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(ct))
        {
            retiring.Add(reader.GetString(0));
        }

        return retiring;
    }
}
