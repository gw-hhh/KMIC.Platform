using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Application.Attachments;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

public sealed class PurgeFileCommandHandler(
    IStoredFileRepository files,
    ITransactionLocks locks,
    IUnfinishedMessageProbe probe,
    IExecutionContext execution,
    TimeProvider time)
    : ICommandHandler<PurgeFileCommand, VersionedResponse<StoredFileId>>
{
    public async ValueTask<Result<VersionedResponse<StoredFileId>>> Handle(PurgeFileCommand command, CancellationToken cancellationToken)
    {
        var loaded = await files.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var file = loaded.Value;
        if (file.Status == StoredFileStatus.Purged || file.Status == StoredFileStatus.Deleting)
        {
            return new VersionedResponse<StoredFileId>(file.Id);
        }

        // 先获取排他锁并探查未完成的关联请求；若存在并发关联则返回临时冲突，避免已提交的关联请求失效
        var locked = await locks.TryAcquireExclusiveAsync(FileAttachments.LockScope, [file.Id.Value], cancellationToken);
        if (locked.Count == 0 || (await probe.FindSubjectsAsync(AttachFiles.MessageName, [file.Id.Value], cancellationToken)).Count > 0)
        {
            return CommonErrors.TransientConflict;
        }

        var by = execution.User.ActualUserId ?? execution.User.UserId ?? UserId.System;
        var purged = file.Purge(command.Reason.Trim(), by, time.GetUtcNow());
        return purged.IsFailure ? purged.Error : new VersionedResponse<StoredFileId>(file.Id);
    }
}
