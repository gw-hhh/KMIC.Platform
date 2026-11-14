using System.Text.Json;

namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 实例心跳里的订阅清单，以及实例是否还活着的判断
/// </summary>
internal static class InstanceSubscriptions
{
    /// <summary>
    /// 超过这么久没有心跳，就当作实例已经不在了。心跳默认 30 秒一次，留出几次的余量
    /// </summary>
    public static readonly TimeSpan AliveWithin = TimeSpan.FromMinutes(2);

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// 读出清单；内容读不出来按没有订阅处理，诊断页不能因为一行坏数据整个打不开
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
