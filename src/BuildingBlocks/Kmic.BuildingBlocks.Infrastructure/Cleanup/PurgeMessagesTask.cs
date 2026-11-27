using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Cleanup;

// 每天清理 Outbox 与 Inbox（Inbox 去重凭证须在来源 Outbox 删除后方可清理）
internal sealed class PurgeMessagesTask(ModuleFrameworkTables tables, DataSources dataSources, IOptions<PurgeOptions> options) : IFrameworkTask
{
    public string Name => "Framework.PurgeMessages";

    public string Cron => "0 0 3 * * ?";

    public string DisplayName => "清理已投递和已处理的消息";

    public async Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct)
    {
        var database = dataSources.Transactions(CallOrigin.ScheduledJob);
        var window = new Dictionary<string, object>(StringComparer.Ordinal) { ["window"] = options.Value.RedeliveryWindow };

        // 先清理 Outbox，使同一次执行中刚删除来源记录的 Inbox 凭证可紧接着被清理
        long outbox = 0;
        foreach (var module in tables.Modules)
        {
            // 投递时间不早于发生时间，附加发生时间条件以命中 (Status, OccurredAt) 索引
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

        // 来源模块未注册于当前实例的凭证无法校验来源状态，予以保留
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
