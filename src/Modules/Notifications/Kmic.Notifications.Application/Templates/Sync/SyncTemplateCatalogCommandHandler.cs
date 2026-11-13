using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Sync;

/// <summary>
/// 处理 <see cref="SyncTemplateCatalogCommand"/>
/// </summary>
public sealed class SyncTemplateCatalogCommandHandler(NotificationTemplateCatalog catalog, ITemplateRepository templates, ITransactionLocks locks)
    : ICommandHandler<SyncTemplateCatalogCommand, TemplateSyncResult>
{
    private static readonly Guid _lockId = new("5f0f4a54-3b3e-4d6e-9c55-6d2d0c8f7a11");

    /// <inheritdoc />
    public async ValueTask<Result<TemplateSyncResult>> Handle(SyncTemplateCatalogCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 别的实例正在同步，它会做完同样的事
        if ((await locks.TryAcquireExclusiveAsync("Notifications.TemplateSync", [_lockId], cancellationToken)).Count == 0)
        {
            return new TemplateSyncResult(0, 0, 0);
        }

        var existing = (await templates.ListAllAsync(cancellationToken)).ToDictionary(t => t.Key, StringComparer.Ordinal);
        int added = 0, updated = 0;
        foreach (var definition in catalog.All)
        {
            var channels = TemplateChannels.From(definition.Channels);
            var hash = TemplateHash.Of(definition);
            if (!existing.Remove(definition.Key, out var template))
            {
                templates.Add(NotificationTemplate.Create(definition.Key, definition.DisplayName, definition.Module, channels, definition.SenderProfile, definition.CarriesSecret, hash));
                added++;
            }
            else if (template.Status == TemplateStatus.Retired || !string.Equals(template.CodeHash, hash, StringComparison.Ordinal))
            {
                template.Synchronize(definition.DisplayName, channels, definition.SenderProfile, definition.CarriesSecret, hash);
                updated++;
            }
        }

        // 剩下的是代码里已经没有的
        var stale = existing.Values.Where(t => t.Status == TemplateStatus.Active).ToList();
        foreach (var template in stale)
        {
            template.Retire();
        }

        return new TemplateSyncResult(added, updated, stale.Count);
    }
}
