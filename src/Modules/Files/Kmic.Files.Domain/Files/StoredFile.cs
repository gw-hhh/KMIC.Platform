using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Files.Domain.Errors;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 一个文件：元数据、状态、引用
/// </summary>
/// <remarks>
/// 内容在对象存储里，这里只有元数据。状态流转记入业务历史，可以追溯文件何时、因何被删除。
/// 批量清理不经聚合的整体加载：把文件改为“删除中”要按关联与清理的互斥协议（排他咨询锁、探查未完成的关联消息、
/// 带状态条件的更新）由基础设施用 SQL 完成，聚合上的 <see cref="Expire" /> 只用于单个文件的人工处理与测试
/// </remarks>
public sealed class StoredFile : AggregateRoot<StoredFileId>
{
    /// <summary>
    /// 一个文件最多被多少个对象引用：防御性上限，防止一个文件的引用无限增长
    /// </summary>
    public const int MaxReferences = 100;

    /// <summary>
    /// 模块生成的文件，上传档案名的前缀：Generated:来源。这类文件没有被关联也可以下载，到期清理
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
    /// 显示用的文件名：去掉路径与控制字符，只作显示
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
    /// 上传档案名；生成的文件是 Generated:来源
    /// </summary>
    public string Profile { get; private set; }

    /// <summary>
    /// 上传的人；系统生成的文件没有
    /// </summary>
    public UserId? UploadedBy { get; private set; }

    /// <summary>
    /// 上传开始的时间
    /// </summary>
    public DateTimeOffset UploadedAt { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StoredFileStatus Status { get; private set; } = null!;

    /// <summary>
    /// 成为清理候选的时间：临时、已解除关联、被拒绝的文件才有，已关联的文件为空
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// 被拒绝的原因：类型不符、超过大小、扫描没通过、写入失败
    /// </summary>
    public string? RejectReason { get; private set; }

    /// <summary>
    /// 彻底删除的时间
    /// </summary>
    public DateTimeOffset? PurgedAt { get; private set; }

    /// <summary>
    /// 彻底删除的人
    /// </summary>
    public UserId? PurgedBy { get; private set; }

    /// <summary>
    /// 彻底删除的原因
    /// </summary>
    public string? PurgeReason { get; private set; }

    /// <summary>
    /// 运维彻底删除历史版本之后登记的时间；为空表示历史版本可能仍在
    /// </summary>
    public DateTimeOffset? HistoryPurgedAt { get; private set; }

    /// <summary>
    /// 彻底删除历史版本的执行记录，如工单号
    /// </summary>
    public string? HistoryPurgeNote { get; private set; }

    /// <summary>
    /// 对象在存储里被删除的时间：只有彻底删除的文件需要，空表示对象还在、等删除工作器处理
    /// </summary>
    /// <remarks>
    /// 只由删除工作器在删除对象之后更新，不经聚合方法；到期清理的文件整行删除，没有这个值
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
    /// 登记一个模块自己生成的文件（导出结果、系统生成的 PDF）：对象已经写进存储，直接可用，到期没有被关联就清理
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
    /// 写入完成：有扫描器时转“扫描中”，否则“可用”，临时文件的清理时间是现在加临时有效期
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
    /// 扫描通过：转“可用”，清理时间从现在重新算
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
    /// 拒绝：上传时类型或大小不符、写入失败，或扫描没通过；保留一小段时间就清理
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
    /// 对象丢失：存储里找不到这个版本的内容，当成被拒绝处理，保留一小段时间就清理
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
    /// 可用或已解除关联的文件转“已关联”；已经是“已关联”的，同一个对象再关联什么也不改（幂等），别的对象再关联只是多一个引用。
    /// 删除中、已彻底删除的文件不能关联：业务事务已经在锁内检查过可关联性，到这里还失败是程序缺陷
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
    /// 业务对象解除关联：引用为零时转“已解除关联”，保留期满才清理；引用不存在时什么也不改
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
    /// 到期进入删除队列：临时、已解除关联、被拒绝的文件
    /// </summary>
    public Result Expire(string reason) => _machine.Fire(this, StoredFileAction.Expire, reason);

    /// <summary>
    /// 彻底删除：除“删除中”之外任何状态都可以，之后不能关联、不能恢复；引用保留，引用方显示“文件已删除”
    /// </summary>
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
    /// 当前状态能否执行某个动作，给“可用动作”判断用
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

        // 对账发现对象丢失：只有应该有内容的这三种状态会遇到
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
