using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Sending;

// 按渠道统计投递积压（无积压时输出 0），最早积压时长包含退避等待时间
internal sealed class DeliveryBacklogGauges(IQueryScopeFactory queries) : IBacklogGauges
{
    private const string Pending = "kmic.notifications.pending";
    private const string OldestAge = "kmic.notifications.oldest_age";

    private static readonly string _pending = DeliveryStatus.Pending.Code;
    private static readonly string _sending = DeliveryStatus.Sending.Code;

    public string MeterName => NotificationsMetrics.MeterName;

    public IReadOnlyList<BacklogGauge> Gauges { get; } =
    [
        new(Pending, "{delivery}", "待发送和发送中的投递数"),
        new(OldestAge, "s", "最早一条待发送投递创建至今的时间"),
    ];

    public async Task<IReadOnlyList<BacklogReading>> ReadAsync(CancellationToken ct)
    {
        var rows = await queries.RunAsync<NotificationsQueryDbContext, List<ChannelBacklog>>(
            CallOrigin.BackgroundJob,
            (db, token) => db.Database.SqlQuery<ChannelBacklog>(
                $"""
                SELECT "Channel" AS "Channel", count(*)::float8 AS "Pending",
                       COALESCE(extract(epoch FROM clock_timestamp() - min("CreatedAt")), 0)::float8 AS "OldestAgeSeconds"
                FROM "Notifications"."Delivery"
                WHERE "Status" IN ({_pending}, {_sending})
                GROUP BY "Channel"
                """).ToListAsync(token),
            ct);

        var readings = new List<BacklogReading>();
        foreach (var channel in DeliveryChannel.All)
        {
            var row = rows.Find(r => string.Equals(r.Channel, channel.Code, StringComparison.Ordinal));
            KeyValuePair<string, object?>[] tags = [new("channel", channel.Code)];
            readings.Add(new BacklogReading(Pending, row?.Pending ?? 0, tags));
            readings.Add(new BacklogReading(OldestAge, row?.OldestAgeSeconds ?? 0, tags));
        }

        return readings;
    }

    private sealed record ChannelBacklog(string Channel, double Pending, double OldestAgeSeconds);
}
