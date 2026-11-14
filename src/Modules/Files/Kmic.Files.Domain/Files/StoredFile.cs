using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Files.Domain.Errors;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 存储文件
/// </summary>
/// <remarks>
/// 内容保存在对象存储中，状态变更记入业务历史
/// </remarks>
public sealed class StoredFile : AggregateRoot<StoredFileId>
{
    /// <summary>
    /// 单文件引用数上限，防止引用集合无限增长
    /// </summary>
    public const int MaxReferences = 100;

    /// <summary>
    /// 生成文件的 Profile 前缀，完整值为 Generated:来源。未关联时也可下载，到期可清理
    /// </summary>
    public const string GeneratedProfilePrefix = "Generated:";

    private static readonly StateMachine<StoredFile, StoredFileStatus, StoredFileAction> _machine = Build();

    private readonly List<FileReference> _references = [];

    private StoredFile(StoredFileId id, string storageKey, string originalName, string profile, UserId? uploadedBy, DateTimeOffset uploadedAt)
        : base(id)
    {
        StorageKey = storageKey;
        OriginalName = originalName;
        Profile = profile;
        UploadedBy = uploadedBy;
        UploadedAt = uploadedAt;
    }

    /// <summary>
    /// 对象在存储里的键，由文件 ID 生成，与文件名无关
    /// </summary>
    public string StorageKey { get; private set; }

    /// <summary>
    /// 显示文件名
    /// </summary>
    public string OriginalName { get; private set; }

    /// <summary>
    /// 识别出的文件类型编码；写入完成之前没有
    /// </summary>
    public string? FileType { get; private set; }

    /// <summary>
    /// 识别出的内容类型，不信任客户端声明的值
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    /// 实际字节数；写入完成之前没有
    /// </summary>
    public long? Size { get; private set; }

    /// <summary>
    /// 内容的 SHA-256，小写十六进制
    /// </summary>
    public string? Sha256 { get; private set; }

    /// <summary>
    /// 存储里对象的版本 ID：开启版本控制的桶才有，恢复与对账用
    /// </summary>
    public string? ObjectVersionId { get; private set; }

    /// <summary>
    /// 上传配置名称
    /// </summary>
    /// <remarks>
    /// 保存上传配置的 Name；生成文件使用 Generated:来源。
    /// </remarks>
    public string Profile { get; private set; }

    /// <summary>
    /// 上传人或生成请求人
    /// </summary>
    public UserId? UploadedBy { get; private set; }

    /// <summary>
    /// 文件登记时间
    /// </summary>
    /// <remarks>
    /// 上传文件在开始上传时记录；生成文件在处理登记消息时记录。
    /// </remarks>
    public DateTimeOffset UploadedAt { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StoredFileStatus Status { get; private set; } = null!;

    /// <summary>
    /// 清理到期时间
    /// </summary>
    /// <remarks>
    /// 到期后成为清理候选，实际清理由文件状态、互斥检查和后台任务决定。
    /// </remarks>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// 拒绝原因
    /// </summary>
    public string? RejectReason { get; private set; }

    /// <summary>
    /// 彻底删除标记时间
    /// </summary>
    public DateTimeOffset? PurgedAt { get; private set; }

    /// <summary>
    /// 彻底删除操作人
    /// </summary>
    public UserId? PurgedBy { get; private set; }

    /// <summary>
    /// 彻底删除的原因
    /// </summary>
    public string? PurgeReason { get; private set; }

    /// <summary>
    /// 历史版本清除登记时间
    /// </summary>
    public DateTimeOffset? HistoryPurgedAt { get; private set; }

    /// <summary>
    /// 历史版本清除记录
    /// </summary>
    public string? HistoryPurgeNote { get; private set; }

    /// <summary>
    /// 对象删除登记时间
    /// </summary>
    /// <remarks>
    /// 删除工作器确认删除成功后登记，包含对象已不存在的情况；为空不代表对象仍在。
    /// 到期清理会删除整条记录；历史版本另行处理。
    /// </remarks>
    public DateTimeOffset? ObjectDeletedAt { get; private set; }

    /// <summary>
    /// 引用它的业务对象
    /// </summary>
    public IReadOnlyList<FileReference> References => _references;

    /// <summary>
    /// 开始上传：登记一行“上传中”，内容随后流式写入存储
    /// </summary>
    public static StoredFile Start(StoredFileId id, string storageKey, string originalName, string profile, UserId? uploadedBy, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);

        var file = new StoredFile(id, storageKey, originalName, profile, uploadedBy, now);
        _machine.Start(file, "开始上传");
        return file;
    }

    /// <summary>
    /// 登记已写入存储的模块生成文件，初始状态为可用，未关联文件到期后可清理
    /// </summary>
    public static StoredFile RegisterGenerated(
        StoredFileId id,
        string storageKey,
        string? objectVersionId,
        string originalName,
        string fileType,
        string contentType,
        long size,
        string sha256,
        string source,
        UserId? requestedBy,
        DateTimeOffset now,
        DateTimeOffset expiresAt)
    {
        var file = Start(id, storageKey, originalName, GeneratedProfilePrefix + source, requestedBy, now);
        file.FinishWrite(fileType, contentType, size, sha256, objectVersionId);
        file.ExpiresAt = expiresAt;
        _machine.Fire(file, StoredFileAction.Complete, "生成的文件已登记");
        return file;
    }

    /// <summary>
    /// 完成上传并设置临时保留期；需要扫描时进入扫描中，否则为可用
    /// </summary>
    public Result CompleteUpload(string fileType, string contentType, long size, string sha256, string? objectVersionId, bool needsScan, DateTimeOffset now, TimeSpan temporaryLifetime)
    {
        if (Status != StoredFileStatus.Uploading)
        {
            return FilesErrors.InvalidState;
        }

        FinishWrite(fileType, contentType, size, sha256, objectVersionId);
        ExpiresAt = now + temporaryLifetime;
        return needsScan
            ? _machine.Fire(this, StoredFileAction.BeginScan, "写入完成，等待扫描")
            : _machine.Fire(this, StoredFileAction.Complete, "写入完成");
    }

    /// <summary>
    /// 扫描通过后转为可用，重新计算临时保留期
    /// </summary>
    public Result PassScan(DateTimeOffset now, TimeSpan temporaryLifetime)
    {
        var fired = _machine.Fire(this, StoredFileAction.PassScan, "扫描通过");
        if (fired.IsSuccess)
        {
            ExpiresAt = now + temporaryLifetime;
        }

        return fired;
    }

    /// <summary>
    /// 将上传或扫描失败的文件标记为被拒绝，保留期满后可清理
    /// </summary>
    public Result Reject(string reason, DateTimeOffset now, TimeSpan retention)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var fired = _machine.Fire(this, StoredFileAction.Reject, reason);
        if (fired.IsSuccess)
        {
            RejectReason = reason;
            ExpiresAt = now + retention;
        }

        return fired;
    }

    /// <summary>
    /// 将对象丢失的文件标记为被拒绝，保留期满后可清理
    /// </summary>
    public Result MarkLost(string reason, DateTimeOffset now, TimeSpan retention)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var fired = _machine.Fire(this, StoredFileAction.Lose, reason);
        if (fired.IsSuccess)
        {
            RejectReason = reason;
            ExpiresAt = now + retention;
        }

        return fired;
    }

    /// <summary>
    /// 被业务对象关联
    /// </summary>
    /// <remarks>
    /// 可用或已解除关联的文件转为已关联；同一业务对象重复关联不新增引用，其他对象关联会增加引用。
    /// 删除中或已彻底删除时禁止关联，调用方须在锁内检查文件是否允许关联
    /// </remarks>
    public Result Attach(string ownerModule, string ownerType, Guid ownerId, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);

        if (Status == StoredFileStatus.Attached)
        {
            if (_references.Exists(r => r.IsOwnedBy(ownerModule, ownerType, ownerId)))
            {
                return Result.Success();
            }
        }
        else
        {
            var fired = _machine.Fire(this, StoredFileAction.Attach, $"被 {ownerModule}.{ownerType} 关联");
            if (fired.IsFailure)
            {
                return Status == StoredFileStatus.Purged ? FilesErrors.Purged : FilesErrors.NotAttachable;
            }

            ExpiresAt = null;
        }

        if (_references.Count >= MaxReferences)
        {
            return FilesErrors.TooManyReferences;
        }

        _references.Add(FileReference.Create(ownerModule, ownerType, ownerId, now));
        return Result.Success();
    }

    /// <summary>
    /// 解除业务引用，最后一个引用解除后进入保留期；引用不存在时直接返回成功
    /// </summary>
    public Result Detach(string ownerModule, string ownerType, Guid ownerId, DateTimeOffset now, TimeSpan retention)
    {
        var reference = _references.Find(r => r.IsOwnedBy(ownerModule, ownerType, ownerId));
        if (reference is null)
        {
            return Result.Success();
        }

        _references.Remove(reference);
        if (_references.Count > 0 || Status != StoredFileStatus.Attached)
        {
            return Result.Success();
        }

        var fired = _machine.Fire(this, StoredFileAction.Detach, "最后一个引用解除");
        if (fired.IsSuccess)
        {
            ExpiresAt = now + retention;
        }

        return fired;
    }

    /// <summary>
    /// 将到期文件转为删除中
    /// </summary>
    /// <remarks>
    /// 仅允许可用、已解除关联或被拒绝状态，到期条件由调用方检查。
    /// 清理可用或已解除关联的文件前，调用方须取得排他咨询锁、探查未完成的关联消息，再重读文件并核对到期时间。
    /// 本方法只校验状态，不获取锁或探查消息
    /// </remarks>
    public Result Expire(string reason) => _machine.Fire(this, StoredFileAction.Expire, reason);

    /// <summary>
    /// 标记文件为已彻底删除
    /// </summary>
    /// <remarks>
    /// 保留引用，禁止再次关联或恢复；引用方显示“文件已删除”。删除中或已彻底删除时拒绝
    /// </remarks>
    public Result Purge(string reason, UserId by, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var fired = _machine.Fire(this, StoredFileAction.Purge, reason);
        if (fired.IsSuccess)
        {
            PurgedAt = now;
            PurgedBy = by;
            PurgeReason = reason;
        }

        return fired;
    }

    /// <summary>
    /// 登记运维已经在对象存储里彻底删除了历史版本
    /// </summary>
    public Result RecordHistoryPurge(string note, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(note);

        if (Status != StoredFileStatus.Purged)
        {
            return FilesErrors.InvalidState;
        }

        HistoryPurgedAt = now;
        HistoryPurgeNote = note;
        return Result.Success();
    }

    /// <summary>
    /// 检查当前状态是否允许指定操作
    /// </summary>
    public Result CanFire(StoredFileAction action) => _machine.CanFire(this, action);

    private static StateMachine<StoredFile, StoredFileStatus, StoredFileAction> Build()
    {
        var builder = StateMachine.Define<StoredFile, StoredFileStatus, StoredFileAction>(f => f.Status, (f, v) => f.Status = v)
            .Initial(StoredFileStatus.Uploading)
            .OnInvalid(FilesErrors.InvalidState)
            .Permit(StoredFileStatus.Uploading, StoredFileAction.Complete, StoredFileStatus.Available)
            .Permit(StoredFileStatus.Uploading, StoredFileAction.BeginScan, StoredFileStatus.Scanning)
            .Permit(StoredFileStatus.Scanning, StoredFileAction.PassScan, StoredFileStatus.Available)
            .Permit(StoredFileStatus.Uploading, StoredFileAction.Reject, StoredFileStatus.Rejected)
            .Permit(StoredFileStatus.Scanning, StoredFileAction.Reject, StoredFileStatus.Rejected)
            .Permit(StoredFileStatus.Available, StoredFileAction.Attach, StoredFileStatus.Attached)
            .Permit(StoredFileStatus.Detached, StoredFileAction.Attach, StoredFileStatus.Attached)
            .Permit(StoredFileStatus.Attached, StoredFileAction.Detach, StoredFileStatus.Detached)
            .Permit(StoredFileStatus.Available, StoredFileAction.Expire, StoredFileStatus.Deleting)
            .Permit(StoredFileStatus.Detached, StoredFileAction.Expire, StoredFileStatus.Deleting)
            .Permit(StoredFileStatus.Rejected, StoredFileAction.Expire, StoredFileStatus.Deleting);

        // 彻底删除：除“删除中”和已经彻底删除的之外，任何状态都可以
        foreach (var from in new[] { StoredFileStatus.Uploading, StoredFileStatus.Scanning, StoredFileStatus.Available, StoredFileStatus.Attached, StoredFileStatus.Detached, StoredFileStatus.Rejected })
        {
            builder.Permit(from, StoredFileAction.Purge, StoredFileStatus.Purged);
        }

        // 对账发现对象丢失
        foreach (var from in new[] { StoredFileStatus.Available, StoredFileStatus.Attached, StoredFileStatus.Detached })
        {
            builder.Permit(from, StoredFileAction.Lose, StoredFileStatus.Rejected);
        }

        return builder.Build();
    }

    private void FinishWrite(string fileType, string contentType, long size, string sha256, string? objectVersionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileType);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        FileType = fileType;
        ContentType = contentType;
        Size = size;
        Sha256 = sha256;
        ObjectVersionId = objectVersionId;
    }
}
