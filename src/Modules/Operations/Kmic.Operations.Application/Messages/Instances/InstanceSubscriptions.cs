using System.Text.Json;

namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 解析实例订阅清单并判断心跳是否有效
/// </summary>
internal static class InstanceSubscriptions
{
    /// <summary>
    /// 心跳有效期，超过后按失联处理
    /// </summary>
    public static readonly TimeSpan AliveWithin = TimeSpan.FromMinutes(2);

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// 解析订阅清单；JSON 无效或为 null 时返回空列表，避免诊断查询失败
    /// </summary>
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
