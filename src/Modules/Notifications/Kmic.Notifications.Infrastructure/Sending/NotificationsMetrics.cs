using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 通知投递指标
/// </summary>
internal sealed class NotificationsMetrics
{
    /// <summary>
    /// Meter 的名称
    /// </summary>
    public const string MeterName = "Kmic.Notifications";

    private readonly Counter<long> _deliveries;

    public NotificationsMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _deliveries = factory.Create(MeterName).CreateCounter<long>("kmic.notifications.deliveries", "{delivery}", "投递的处理结果数");
    }

    /// <summary>
    /// 一次领取的投递处理结束
    /// </summary>
    public void Delivered(string channel, string result) =>
        _deliveries.Add(1, new TagList { { "channel", channel }, { "result", result } });
}
