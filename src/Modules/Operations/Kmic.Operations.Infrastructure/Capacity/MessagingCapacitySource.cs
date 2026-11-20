using Kmic.BuildingBlocks.Application.Settings;
using Kmic.Operations.Application.MessagingCapacity;

namespace Kmic.Operations.Infrastructure.Capacity;

/// <summary>
/// 容量保护要的数据库已分配容量：读 Operations 声明的系统设置，每次都读最新值
/// </summary>
internal sealed class MessagingCapacitySource(IFreshSettings settings) : IMessagingCapacitySource
{
    public async Task<MessagingCapacity?> ReadAsync(CancellationToken ct)
    {
        var setting = await settings.GetAsync<MessagingCapacitySetting>(ct);

        // 0 表示没有设置
        return setting is { SavedAt: { } savedAt, Value.CapacityBytes: > 0 and var bytes } ? new MessagingCapacity(bytes, savedAt) : null;
    }
}
