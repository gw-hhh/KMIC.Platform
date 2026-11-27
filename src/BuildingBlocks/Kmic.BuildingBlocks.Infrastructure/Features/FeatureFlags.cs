using Kmic.BuildingBlocks.Application.Features;
using Microsoft.FeatureManagement;

namespace Kmic.BuildingBlocks.Infrastructure.Features;

internal sealed class FeatureFlags(IFeatureManager manager) : IFeatureFlags
{
    public async ValueTask<bool> IsEnabledAsync(string name, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await manager.IsEnabledAsync(name);
    }

    public async ValueTask<IReadOnlyList<string>> EnabledAsync(CancellationToken ct)
    {
        var enabled = new List<string>();
        await foreach (var name in manager.GetFeatureNamesAsync().WithCancellation(ct))
        {
            if (await manager.IsEnabledAsync(name))
            {
                enabled.Add(name);
            }
        }

        enabled.Sort(StringComparer.Ordinal);
        return enabled;
    }
}
