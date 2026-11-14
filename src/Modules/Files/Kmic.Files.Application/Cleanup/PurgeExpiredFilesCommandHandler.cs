using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Application.Attachments;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 将到期文件加入删除队列
/// </summary>
public sealed class PurgeExpiredFilesCommandHandler(
    IStoredFileRepository files,
    ITransactionLocks locks,
    IUnfinishedMessageProbe probe,
    IOptions<FilesOptions> options,
    TimeProvider time)
    : ICommandHandler<PurgeExpiredFilesCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeExpiredFilesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = time.GetUtcNow();
        var batch = options.Value.PurgeBatchSize;
        var marked = 0;

        // 被拒绝的文件不可关联，无需获取关联锁
        var rejected = await files.FindExpiredAsync(StoredFileStatus.Rejected, now, batch, cancellationToken);
        marked += await ExpireAsync(rejected, "被拒绝的文件到期", cancellationToken);

        // 临时文件和已解除关联的文件可能存在并发关联，须先检查互斥条件；共用本批剩余额度
        var remaining = batch - rejected.Count;
        if (remaining > 0)
        {
            var temporary = await files.FindExpiredAsync(StoredFileStatus.Available, now, remaining, cancellationToken);
            var detached = await files.FindExpiredAsync(StoredFileStatus.Detached, now, Math.Max(remaining - temporary.Count, 0), cancellationToken);
            marked += await ExpireGuardedAsync(temporary, "临时文件到期", cancellationToken);
            marked += await ExpireGuardedAsync(detached, "解除关联后的保留期满", cancellationToken);
        }

        return new ScheduledJobOutcome(marked);
    }

    // 取得排他锁后先探查未完成的关联请求，再重读文件并核对到期时间和状态；锁持有至命令事务结束
    private async Task<int> ExpireGuardedAsync(IReadOnlyList<StoredFileId> candidates, string reason, CancellationToken ct)
    {
        if (candidates.Count == 0)
        {
            return 0;
        }

        var locked = await locks.TryAcquireExclusiveAsync(FileAttachments.LockScope, [.. candidates.Select(c => c.Value)], ct);
        if (locked.Count == 0)
        {
            return 0;
        }

        var pending = await probe.FindSubjectsAsync(AttachFiles.MessageName, [.. locked], ct);
        return await ExpireAsync([.. candidates.Where(c => locked.Contains(c.Value) && !pending.Contains(c.Value))], reason, ct);
    }

    private async Task<int> ExpireAsync(IReadOnlyList<StoredFileId> ids, string reason, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var expired = 0;
        foreach (var id in ids)
        {
            var loaded = await files.GetForUpdateAsync(id, null, ct);

            // 候选状态可能已变化；重读后仅处理仍到期且允许清理的文件
            if (loaded.IsSuccess && loaded.Value.ExpiresAt <= now && loaded.Value.Expire(reason).IsSuccess)
            {
                expired++;
            }
        }

        return expired;
    }
}
