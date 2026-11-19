using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 核对文件记录与存储对象并处理差异
/// </summary>
public sealed class ReconcileStorageCommandHandler(
    IStorageInventory storage,
    IStoredFileRepository files,
    IStorageIssueRepository issues,
    IUnfinishedMessageProbe probe,
    IOptions<FilesOptions> options,
    TimeProvider time)
    : ICommandHandler<ReconcileStorageCommand, ScheduledJobOutcome>
{
    private const string LostReason = "对象丢失";

    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(ReconcileStorageCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var cutoff = now - options.Value.ReconcileMinAge;

        // 先收集差异并校验结果；空清单异常或差异数量超限时，不执行删除和问题登记
        var scan = await ScanStorageAsync(cutoff, cancellationToken);
        var missing = await FindMissingAsync(scan.Present, cutoff, cancellationToken);
        var limit = options.Value.ReconcileMaxChanges;
        var listedNothing = scan.Present.Count == 0 && missing.Examined > 0;
        if (listedNothing || scan.Orphans.Count > limit || missing.Files.Count > limit)
        {
            return FilesErrors.ReconcileSuspicious;
        }

        foreach (var orphan in scan.Orphans)
        {
            await storage.DeleteAsync(orphan.Key, cancellationToken);
        }

        var recorded = await RecordMissingAsync(missing.Files, now, cancellationToken);
        return new ScheduledJobOutcome(
            scan.Orphans.Count + recorded,
            $"检查了 {scan.Present.Count} 个对象、{missing.Examined} 个文件；删除存储里多出的对象 {scan.Orphans.Count} 个，记录存储问题 {recorded} 个");
    }

    private async Task<StorageScan> ScanStorageAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var present = new HashSet<Guid>();
        var orphans = new List<InventoryObject>();
        var batch = new List<InventoryObject>(options.Value.ReconcileBatchSize);
        await foreach (var item in storage.ListAsync(ct))
        {
            present.Add(item.FileId);
            if (item.CreatedAt is { } created && created <= cutoff)
            {
                batch.Add(item);
            }

            if (batch.Count >= options.Value.ReconcileBatchSize)
            {
                orphans.AddRange(await FindOrphansAsync(batch, ct));
                batch.Clear();
            }
        }

        orphans.AddRange(await FindOrphansAsync(batch, ct));
        return new StorageScan(present, orphans);
    }

    // 先查未完成的登记消息，再查文件记录，避免登记在两次查询之间提交时误删对象
    private async Task<IReadOnlyList<InventoryObject>> FindOrphansAsync(List<InventoryObject> batch, CancellationToken ct)
    {
        if (batch.Count == 0)
        {
            return [];
        }

        var pending = await probe.FindSubjectsAsync(RegisterGeneratedFile.MessageName, [.. batch.Select(o => o.FileId)], ct);
        var known = await files.FindExistingAsync([.. batch.Select(o => StoredFileId.From(o.FileId))], ct);
        return [.. batch.Where(o => !pending.Contains(o.FileId) && !known.Contains(StoredFileId.From(o.FileId)))];
    }

    // 清单与文件记录可能不同步，逐个确认当前对象是否缺失，避免误报
    private async Task<MissingFiles> FindMissingAsync(HashSet<Guid> present, DateTimeOffset cutoff, CancellationToken ct)
    {
        var missing = new List<StoredObjectRef>();
        var examined = 0;
        StoredFileId? after = null;
        while (true)
        {
            var page = await files.FindExpectedObjectsAsync(after, cutoff, options.Value.ReconcileBatchSize, ct);
            if (page.Count == 0)
            {
                return new MissingFiles(examined, missing);
            }

            examined += page.Count;
            after = page[^1].Id;
            foreach (var candidate in page.Where(p => !present.Contains(p.Id.Value)))
            {
                if (!await storage.ExistsAsync(candidate.StorageKey, null, ct))
                {
                    missing.Add(candidate);
                }
            }
        }
    }

    private async Task<int> RecordMissingAsync(List<StoredObjectRef> missing, DateTimeOffset now, CancellationToken ct)
    {
        if (missing.Count == 0)
        {
            return 0;
        }

        var open = await issues.FindOpenFileIdsAsync([.. missing.Select(m => m.Id)], ct);
        var recorded = 0;
        foreach (var item in missing.Where(m => !open.Contains(m.Id)))
        {
            var restorable = item.ObjectVersionId is not null && await storage.ExistsAsync(item.StorageKey, item.ObjectVersionId, ct);
            if (!restorable && !await MarkLostAsync(item.Id, now, ct))
            {
                continue;
            }

            issues.Add(StorageIssue.Found(item.Id, restorable ? StorageIssueKind.RestorableMissing : StorageIssueKind.LostMissing, item.ObjectVersionId, now));
            recorded++;
        }

        return recorded;
    }

    // 重读文件并校验状态，避免将已进入删除流程的文件标记为丢失
    private async Task<bool> MarkLostAsync(StoredFileId id, DateTimeOffset now, CancellationToken ct)
    {
        var loaded = await files.GetForUpdateAsync(id, null, ct);
        return loaded.IsSuccess && loaded.Value.MarkLost(LostReason, now, options.Value.RejectedRetention).IsSuccess;
    }

    private sealed record StorageScan(HashSet<Guid> Present, List<InventoryObject> Orphans);

    private sealed record MissingFiles(int Examined, List<StoredObjectRef> Files);
}
