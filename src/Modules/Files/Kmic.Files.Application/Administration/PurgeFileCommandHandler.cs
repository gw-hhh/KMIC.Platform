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

/// <summary>
/// 处理 <see cref="PurgeFileCommand"/>
/// </summary>
public sealed class PurgeFileCommandHandler(
    IStoredFileRepository files,
    ITransactionLocks locks,
    IUnfinishedMessageProbe probe,
    IExecutionContext execution,
    TimeProvider time)
    : ICommandHandler<PurgeFileCommand, VersionedResponse<StoredFileId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<StoredFileId>>> Handle(PurgeFileCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

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

        // 和清理一样走互斥协议：业务正在关联它（持有共享锁）或者还有关联请求在路上，这时彻底删除会让那条关联请求落空，稍后再试
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
