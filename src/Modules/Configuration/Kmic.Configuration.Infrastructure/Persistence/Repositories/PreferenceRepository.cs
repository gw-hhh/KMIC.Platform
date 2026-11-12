using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence.Repositories;

internal sealed class PreferenceRepository(ConfigurationDbContext context)
    : EfRepository<ConfigurationDbContext, PreferenceValue, PreferenceValueId>(context), IPreferenceRepository
{
    protected override Error NotFound => ConfigurationErrors.PreferenceNotFound;

    public async Task<PreferenceValue?> FindForUpdateAsync(string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, CancellationToken ct) =>
        await Context.Set<PreferenceValue>()
            .SingleOrDefaultAsync(v => v.Key == key && v.Layer == layer && v.RoleId == roleId && v.UserId == userId, ct);
}
