using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 处理 <see cref="ListSettingsQuery"/>
/// </summary>
public sealed class ListSettingsQueryHandler(IConfigurationQueryDb db, SystemSettingCatalog catalog, IAllowedActions<SettingTarget> allowedActions) : IQueryHandler<ListSettingsQuery, SettingsResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<SettingsResponse>> Handle(ListSettingsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var stored = await db.SettingValues.Select(v => new { v.Key, v.Value, v.ConcurrencyVersion }).ToDictionaryAsync(v => v.Key, StringComparer.Ordinal, cancellationToken);
        var settings = catalog.All
            .OrderBy(d => d.Module, StringComparer.Ordinal)
            .ThenBy(d => d.Group, StringComparer.Ordinal)
            .ThenBy(d => d.Key, StringComparer.Ordinal)
            .Select(d => SettingItems.Build(d, stored.TryGetValue(d.Key, out var row) ? (row.Value, row.ConcurrencyVersion) : null, allowedActions))
            .ToList();
        var obsolete = stored.Values
            .Where(v => catalog.Find(v.Key) is null)
            .OrderBy(v => v.Key, StringComparer.Ordinal)
            .Select(v => new ObsoleteSettingItem(v.Key, DeclaredValues.ElementOf(v.Value), v.ConcurrencyVersion))
            .ToList();
        return new SettingsResponse(settings, obsolete);
    }
}
