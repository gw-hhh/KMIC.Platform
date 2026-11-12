using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 处理 <see cref="GetSettingQuery"/>
/// </summary>
public sealed class GetSettingQueryHandler(IConfigurationQueryDb db, SystemSettingCatalog catalog, IAllowedActions<SettingTarget> allowedActions) : IQueryHandler<GetSettingQuery, SettingItem>
{
    /// <inheritdoc />
    public async ValueTask<Result<SettingItem>> Handle(GetSettingQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var definition = catalog.Find(query.Key);
        if (definition is null)
        {
            return ConfigurationErrors.SettingNotFound;
        }

        var row = await db.SettingValues.Where(v => v.Key == definition.Key).Select(v => new { v.Value, v.ConcurrencyVersion }).SingleOrDefaultAsync(cancellationToken);
        return SettingItems.Build(definition, row is null ? null : (row.Value, row.ConcurrencyVersion), allowedActions);
    }
}
