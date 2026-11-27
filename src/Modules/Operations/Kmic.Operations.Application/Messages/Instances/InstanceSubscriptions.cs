using System.Text.Json;

namespace Kmic.Operations.Application.Messages.Instances;

internal static class InstanceSubscriptions
{
    // 心跳有效期，超时判定为失联
    public static readonly TimeSpan AliveWithin = TimeSpan.FromMinutes(2);

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<InstanceSubscription> Read(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<InstanceSubscription>>(json, _options) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static bool IsAlive(MessagingInstanceRow row, DateTimeOffset now) => now - row.HeartbeatAt <= AliveWithin;
}
