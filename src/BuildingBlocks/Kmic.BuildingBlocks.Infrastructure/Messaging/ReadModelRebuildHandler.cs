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

// 读模型重建处理器：先全量扫描源端快照，再核对并标记本地多余行
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

        // 框架内置作业上下文固定为 JobContext
        var pages = context as JobContext ?? throw new InvalidOperationException("读模型重建作业必须由框架作业执行器运行。");
        var definition = subscriptions.All.FirstOrDefault(s => string.Equals(s.Name, job.Subscription, StringComparison.Ordinal) && s.Rebuild is not null);
        if (definition?.Rebuild is not { } rebuild)
        {
            return CommonErrors.JobFailed.WithDetail($"当前实例未声明订阅 {job.Subscription} 的读模型重建方式。");
        }

        var settings = options.Value.Rebuild;
        // 运维确认收尾的作业跳过全量扫描阶段（扫描已在前序作业中完成）
        var state = job.ConfirmMissing
            ? new RebuildCheckpoint(true, null, job.Scanned)
            : context.GetCheckpoint<RebuildCheckpoint>() ?? new RebuildCheckpoint(false, null, 0);
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
        var limit = job.ConfirmMissing ? long.MaxValue : Math.Max(settings.MaxMissingCount, (long)(total * settings.MaxMissingRatio));
        var done = new RebuildCheckpoint(true, null, scanned);

        // 候选项若在源端仍存在则按重建逻辑覆盖写入；若源端已缺失则先行记录，待确认未超阈值后再统一置为终态
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

    // 已处于终态的记录不作覆盖；若源端仍返回该记录，可能存在终态误标，仅记录日志交由运维核实
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
