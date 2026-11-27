using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Kmic.Notifications.Infrastructure.Sending;

internal sealed class NotificationsMetrics
{
    public const string MeterName = "Kmic.Notifications";

    private readonly Counter<long> _deliveries;

    public NotificationsMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _deliveries = factory.Create(MeterName).CreateCounter<long>("kmic.notifications.deliveries", "{delivery}", "投递的处理结果数");
    }

    public void Delivered(string channel, string result) =>
        _deliveries.Add(1, new TagList { { "channel", channel }, { "result", result } });
}
