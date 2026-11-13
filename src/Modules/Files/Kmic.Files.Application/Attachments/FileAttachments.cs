using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// <see cref="IFileAttachments"/> 的实现：关联与清理的互斥协议
/// </summary>
/// <remarks>
/// 步骤：按文件 ID 取事务级共享咨询锁；取到锁之后检查文件现在能不能关联，不经缓存、不看到期时间；
/// 发 AttachFiles，和业务关联行同一个事务提交，锁持有到提交。清理一方取排他锁、探查未完成的关联消息后才删除，
/// 所以到期的文件在业务事务提交之前不会被删。
/// 检查要用系统身份（它是只给系统调用的契约查询），所以另开一个系统身份的作用域去调；读到的是已经提交的状态，
/// 清理改状态的事务先提交再放锁，我们取到共享锁时它已经提交了
/// </remarks>
public sealed class FileAttachments(
    ITransactionLocks locks,
    IExecutionScopeFactory scopes,
    IIntegrationEventPublisher events,
    IExecutionContext execution,
    IOptions<FilesOptions> options) : IFileAttachments
{
    /// <summary>
    /// 咨询锁的范围名，清理一方用同样的名字取排他锁
    /// </summary>
    public const string LockScope = "Files";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FileSnapshot>>> AttachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);
        ArgumentNullException.ThrowIfNull(fileIds);

        var ids = fileIds.Distinct().ToList();
        if (ids.Count > options.Value.MaxFilesPerCommand)
        {
            return FilesErrors.TooManyFiles;
        }

        if (ids.Count == 0)
        {
            return new List<FileSnapshot>();
        }

        var locked = await locks.AcquireSharedAsync(LockScope, ids, TimeSpan.FromSeconds(options.Value.AttachLockTimeoutSeconds), ct);
        if (locked.IsFailure)
        {
            return locked.Error;
        }

        var uploader = execution.User.ActualUserId ?? execution.User.UserId ?? UserId.System;
        var attachable = await CheckAsSystemAsync(ids, uploader, ct);
        if (attachable.Count != ids.Count)
        {
            return FilesErrors.NotAttachable;
        }

        events.Send(new AttachFiles(ids, ownerModule, ownerType, ownerId));
        var byId = attachable.ToDictionary(s => s.FileId);
        return ids.Select(id => byId[id]).ToList();
    }

    // 构造函数注入的服务带着调用者的身份；检查必须用系统身份，只能从刚建的系统作用域里取。这是唯一的例外，所以逐处豁免
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "检查要用系统身份，服务只能从系统作用域里取，不能构造函数注入")]
    private async Task<IReadOnlyList<FileSnapshot>> CheckAsSystemAsync(IReadOnlyCollection<Guid> ids, UserId uploader, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.ModuleCall, CurrentUser.System());
        return await scope.ServiceProvider.GetRequiredService<IFileAttachability>().CheckAsync(ids, uploader.Value, ct);
    }

    /// <inheritdoc />
    public Task DetachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);
        ArgumentNullException.ThrowIfNull(fileIds);

        var ids = fileIds.Distinct().ToList();
        if (ids.Count > 0)
        {
            events.Send(new DetachFiles(ids, ownerModule, ownerType, ownerId));
        }

        return Task.CompletedTask;
    }
}
