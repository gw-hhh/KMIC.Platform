using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 每天按保留规则清理 Outbox 与 Inbox
/// </summary>
/// <remarks>
/// Outbox 已投递的消息过了补投窗口才删；Inbox 已处理的行是去重凭证，要等来源的 Outbox 行删掉之后才能删。
/// 两边各按天数删会出问题：某个订阅方一直失败、来源行进了死信，其他订阅方的凭证先被删了，
/// 运维重试死信时消息会再写进它们的 Inbox 又执行一遍。未投递、死信、放弃的消息都不自动删
/// </remarks>
internal sealed class PurgeMessagesTask(ModuleFrameworkTables tables, DataSources dataSources, IOptions<PurgeOptions> options) : IFrameworkTask
{
    public string Name => "Framework.PurgeMessages";

    public string Cron => "0 0 3 * * ?";

    public string DisplayName => "清理已投递和已处理的消息";

    public async Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct)
    {
        var database = dataSources.Transactions(CallOrigin.ScheduledJob);
        var window = new Dictionary<string, object>(StringComparer.Ordinal) { ["window"] = options.Value.RedeliveryWindow };

        // 先删 Outbox：同一次执行里，刚删掉来源行的凭证接着就能删
        long outbox = 0;
        foreach (var module in tables.Modules)
        {
            // 投递时刻不早于发生时刻，多带一个发生时刻的条件是为了用上 (Status, OccurredAt) 索引
            outbox += await BatchDelete.RunAsync(
                database,
                tables.For(module).Outbox,
                $"""
                t."Status" = {MessageStatus.Done}
                AND t."OccurredAt" < clock_timestamp() - @window AND t."DeliveredAt" < clock_timestamp() - @window
                """,
                window,
                options.Value.BatchSize,
                ct);
        }

        // 来源模块不在本实例里的凭证无法判断，留着
        long inbox = 0;
        foreach (var module in tables.Modules)
        {
            foreach (var source in tables.Modules)
            {
                inbox += await BatchDelete.RunAsync(
                    database,
                    tables.For(module).Inbox,
                    $"""
                    t."Status" = {MessageStatus.Done} AND t."SourceModule" = @source AND t."ReceivedAt" < clock_timestamp() - @window
                    AND NOT EXISTS (SELECT 1 FROM {tables.For(source).Outbox} o WHERE o."Id" = t."MessageId")
                    """,
                    new Dictionary<string, object>(window, StringComparer.Ordinal) { ["source"] = source },
                    options.Value.BatchSize,
                    ct);
            }
        }

        return new ScheduledJobOutcome(checked((int)(outbox + inbox)), $"Outbox {outbox} 条，Inbox {inbox} 条");
    }
}
