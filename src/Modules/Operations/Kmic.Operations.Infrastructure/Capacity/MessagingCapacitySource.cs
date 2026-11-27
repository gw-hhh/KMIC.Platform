using Kmic.BuildingBlocks.Application.Settings;
using Kmic.Operations.Application.MessagingCapacity;

namespace Kmic.Operations.Infrastructure.Capacity;

internal sealed class MessagingCapacitySource(IFreshSettings settings) : IMessagingCapacitySource
{
    public async Task<MessagingCapacity?> ReadAsync(CancellationToken ct)
    {
        var setting = await settings.GetAsync<MessagingCapacitySetting>(ct);

        // 0 表示未配置
        return setting is { SavedAt: { } savedAt, Value.CapacityBytes: > 0 and var bytes } ? new MessagingCapacity(bytes, savedAt) : null;
    }
}
