using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 处理 <see cref="ListPreferenceDefinitionsQuery"/>
/// </summary>
public sealed class ListPreferenceDefinitionsQueryHandler(IConfigurationQueryDb db, PreferenceCatalog catalog, IRoleReferences roles)
    : IQueryHandler<ListPreferenceDefinitionsQuery, PreferenceDefinitionsResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<PreferenceDefinitionsResponse>> Handle(ListPreferenceDefinitionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.PreferenceValues
            .Where(v => v.Layer != PreferenceLayer.UserRole)
            .Select(v => new LayerRow(v.Key, v.RoleId, v.Value, v.ConcurrencyVersion))
            .ToListAsync(cancellationToken);
        var roleNames = await roles.ResolveAsync([.. rows.Where(r => r.RoleId is not null).Select(r => r.RoleId!.Value.Value).Distinct()], cancellationToken);

        var byKey = rows.ToLookup(r => r.Key, StringComparer.Ordinal);
        var declared = catalog.All.Select(d => Build(PreferenceSubject.Find(catalog, d.Key).Value, d.Module, DeclaredValues.DefaultOf(d), byKey[d.Key], roleNames));
        var families = rows
            .Select(r => r.Key)
            .Distinct(StringComparer.Ordinal)
            .Where(k => catalog.Find(k) is null && PreferenceFamilies.Find(k) is not null)
            .Select(k => Build(PreferenceSubject.Find(catalog, k).Value, null, null, byKey[k], roleNames));

        var items = declared.Concat(families)
            .OrderBy(i => i.IsFamily)
            .ThenBy(i => i.Module, StringComparer.Ordinal)
            .ThenBy(i => i.Key, StringComparer.Ordinal)
            .ToList();
        return new PreferenceDefinitionsResponse(items);
    }

    private static PreferenceDefinitionItem Build(
        PreferenceSubject subject,
        string? module,
        JsonElement? defaultValue,
        IEnumerable<LayerRow> rows,
        IReadOnlyDictionary<Guid, ReferenceInfo> roleNames)
    {
        var system = rows.FirstOrDefault(r => r.RoleId is null);
        var byRole = rows
            .Where(r => r.RoleId is not null)
            .Select(r => new PreferenceLayerValue(DeclaredValues.ElementOf(r.Value), r.Version, r.RoleId!.Value.Value, roleNames.GetValueOrDefault(r.RoleId.Value.Value)?.DisplayName))
            .OrderBy(v => v.RoleName, StringComparer.Ordinal)
            .ToList();
        return new PreferenceDefinitionItem(
            subject.Key,
            subject.DisplayName,
            module,
            subject.IsFamily,
            subject.LayerList(),
            defaultValue,
            system is null ? null : new PreferenceLayerValue(DeclaredValues.ElementOf(system.Value), system.Version, null, null),
            byRole);
    }

    private sealed record LayerRow(string Key, RoleId? RoleId, string Value, long Version);
}
