using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Sync;

/// <summary>
/// 处理 <see cref="SyncDictionaryTypesCommand"/>
/// </summary>
public sealed class SyncDictionaryTypesCommandHandler(DictionaryTypeCatalog catalog, IDictionaryTypeRepository types, IAfterCommit afterCommit)
    : ICommandHandler<SyncDictionaryTypesCommand, DictionarySyncResult>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionarySyncResult>> Handle(SyncDictionaryTypesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await types.LockCatalogSyncAsync(cancellationToken);
        var existing = (await types.ListWithoutItemsAsync(cancellationToken)).ToDictionary(t => t.Code, StringComparer.Ordinal);
        int registered = 0, updated = 0, retired = 0;
        foreach (var declared in catalog.All)
        {
            if (existing.TryGetValue(declared.Code, out var type))
            {
                if (type.Synchronize(declared.DisplayName, declared.IsHierarchical))
                {
                    updated++;
                }
            }
            else
            {
                types.Add(DictionaryType.Register(declared.Code, declared.Module, declared.DisplayName, declared.IsHierarchical));
                registered++;
            }
        }

        foreach (var type in existing.Values.Where(t => !catalog.Contains(t.Code)))
        {
            if (type.Retire())
            {
                retired++;
            }
        }

        if (registered + updated + retired > 0)
        {
            afterCommit.InvalidateTags(ConfigurationCacheKeys.DictionaryTag);
        }

        return new DictionarySyncResult(registered, updated, retired);
    }
}
