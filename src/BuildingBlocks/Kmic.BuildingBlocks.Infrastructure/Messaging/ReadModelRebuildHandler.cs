using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 重建读模型：先扫描源端的全部快照，再向源端确认本地多出来的行
/// </summary>
/// <remarks>
/// 实时同步照常进行，两边写同一行时留下版本高的。每一页一个事务，和检查点一起提交，
/// 中断后从检查点接着做。收尾阶段每次都从头开始，已经确认过的行不再是候选，重做不会出错
/// </remarks>
internal sealed class ReadModelRebuildHandler(
    SubscriptionCatalog subscriptions,
    RebuildRunStore runs,
    IServiceProvider services,
    IOptions<MessagingOptions> options,
    FrameworkMetrics metrics,
    ILogger<ReadModelRebuildHandler> logger) : IJobHandler<ReadModelRebuildJob>
{
    public async Task<Result<JobOutcome>> ExecuteAsync(ReadModelRebuildJob job, IJobContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);

        // 框架作业的上下文都是 JobContext，没有别的实现
        var pages = context as JobContext ?? throw new InvalidOperationException("重建作业要由框架的作业执行器运行。");
        var definition = subscriptions.All.FirstOrDefault(s => string.Equals(s.Name, job.Subscription, StringComparison.Ordinal) && s.Rebuild is not null);
        if (definition?.Rebuild is not { } rebuild)
        {
            return CommonErrors.JobFailed.WithDetail($"本实例没有声明订阅 {job.Subscription} 的重建方式。");
        }

        var settings = options.Value.Rebuild;
        var state = context.GetCheckpoint<RebuildCheckpoint>() ?? new RebuildCheckpoint(false, null, 0);
        var scanned = state.Scanned;

        if (!state.ScanFinished)
        {
            var after = state.After;
            do
            {
                var page = await rebuild.GetPageAsync(services, after, settings.PageSize, ct);
                scanned += page.Rows.Count;
                var written = await pages.WritePageAsync(
                    async (scope, token) => await ScanAsync(scope, rebuild, page.Rows, job, definition.Module, token),
                    new RebuildCheckpoint(page.Next is null, page.Next, scanned),
                    scanned);
                if (written.IsFailure)
                {
                    return written.Error;
                }

                after = page.Next;
            }
            while (after is not null);
        }

        return await FinishAsync(pages, job, definition, rebuild, scanned, ct);
    }

    private async Task<Result<JobOutcome>> FinishAsync(
        JobContext pages, ReadModelRebuildJob job, SubscriptionDefinition definition, IReadModelRebuild rebuild, long scanned, CancellationToken ct)
    {
        var settings = options.Value.Rebuild;
        var total = await rebuild.CountAsync(services, ct);
        var limit = Math.Max(settings.MaxMissingCount, (long)(total * settings.MaxMissingRatio));
        var done = new RebuildCheckpoint(true, null, scanned);

        // 候选里源端还有的，按重建写入处理；没有的先记下来，数量不超限才置终态
        var missing = new List<ReadModelCandidate>();
        Guid? last = null;
        while (missing.Count <= limit)
        {
            var candidates = await rebuild.ListCandidatesAsync(services, job.RunId, last, settings.PageSize, ct);
            if (candidates.Count == 0)
            {
                break;
            }

            last = candidates[^1].Id;
            var found = await rebuild.GetRowsAsync(services, [.. candidates.Select(c => c.Id)], ct);
            var foundIds = found.Select(r => r.Id).ToHashSet();
            missing.AddRange(candidates.Where(c => !foundIds.Contains(c.Id)));
            if (found.Count > 0)
            {
                var written = await pages.WritePageAsync(async (scope, token) => await ScanAsync(scope, rebuild, found, job, definition.Module, token), done, scanned);
                if (written.IsFailure)
                {
                    return written.Error;
                }
            }
        }

        if (missing.Count > limit)
        {
            await runs.PauseAsync(definition.Module, job.RunId, scanned, missing.Count, ct);
            metrics.RebuildPaused(definition.Module, rebuild.ReadModel.Name);
            MessagingLog.RebuildPaused(logger, definition.Name, missing.Count, limit);
            return new JobOutcome(Summary: new RebuildSummary(scanned, 0, missing.Count, AwaitingConfirmation: true));
        }

        long marked = 0;
        foreach (var chunk in missing.Chunk(settings.PageSize))
        {
            var written = await pages.WritePageAsync(async (scope, token) => marked += await rebuild.MarkMissingAsync(scope, chunk, token), done, scanned);
            if (written.IsFailure)
            {
                return written.Error;
            }
        }

        await runs.CompleteAsync(definition.Module, definition.Name, job.RunId, scanned, missing.Count, ct);
        MessagingLog.RebuildFinished(logger, definition.Name, scanned, marked);
        return new JobOutcome(Summary: new RebuildSummary(scanned, marked, missing.Count, AwaitingConfirmation: false));
    }

    // 终态的行不修改。源端还返回它说明终态可能是误标，只记日志，由运维核实
    private async Task ScanAsync(IServiceProvider scope, IReadModelRebuild rebuild, IReadOnlyList<IReadModel> rows, ReadModelRebuildJob job, string module, CancellationToken ct)
    {
        var skipped = 0;
        foreach (var row in rows)
        {
            if (!await rebuild.ScanAsync(scope, row, job.RunId, ct))
            {
                skipped++;
            }
        }

        if (skipped > 0)
        {
            metrics.TerminalConflicts(module, rebuild.ReadModel.Name, skipped);
            MessagingLog.RebuildTerminalRowsReturned(logger, job.Subscription, skipped);
        }
    }
}
