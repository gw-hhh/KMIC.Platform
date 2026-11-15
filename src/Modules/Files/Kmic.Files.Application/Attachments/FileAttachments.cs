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
/// 为业务对象关联文件或解除文件关联
/// </summary>
/// <remarks>
/// 关联检查使用独立的系统作用域，仅读取已提交数据
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
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "实现 IFileAttachments.AttachAsync，取锁并校验后构造关联请求")]
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

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "须从新建的系统作用域解析服务，确保以系统身份检查关联条件")]
    private async Task<IReadOnlyList<FileSnapshot>> CheckAsSystemAsync(IReadOnlyCollection<Guid> ids, UserId uploader, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.ModuleCall, CurrentUser.System());
        return await scope.ServiceProvider.GetRequiredService<IFileAttachability>().CheckAsync(ids, uploader.Value, ct);
    }

    /// <inheritdoc />
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "实现 IFileAttachments.DetachAsync，需在业务事务中构造解除关联请求")]
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
