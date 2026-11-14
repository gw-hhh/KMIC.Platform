using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 对账发现的存储问题：库里有记录，存储里的对象找不到
/// </summary>
/// <remarks>
/// 由对账任务创建，同一个文件有待处理的问题时不重复创建。运维按运维手册把对象还原之后，在页面上标为已解决
/// </remarks>
public sealed class StorageIssue : AggregateRoot<StorageIssueId>
{
    private StorageIssue(StorageIssueId id, StoredFileId fileId, StorageIssueKind kind, string? objectVersionId, DateTimeOffset foundAt)
        : base(id)
    {
        FileId = fileId;
        Kind = kind;
        ObjectVersionId = objectVersionId;
        FoundAt = foundAt;
    }

    /// <summary>
    /// 出问题的文件
    /// </summary>
    public StoredFileId FileId { get; private set; }

    /// <summary>
    /// 种类
    /// </summary>
    public StorageIssueKind Kind { get; private set; }

    /// <summary>
    /// 发现时文件记录的对象版本
    /// </summary>
    public string? ObjectVersionId { get; private set; }

    /// <summary>
    /// 发现的时间
    /// </summary>
    public DateTimeOffset FoundAt { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StorageIssueStatus Status { get; private set; } = StorageIssueStatus.Open;

    /// <summary>
    /// 标为已解决的时间
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>
    /// 标为已解决的人
    /// </summary>
    public UserId? ResolvedBy { get; private set; }

    /// <summary>
    /// 处理说明，如还原的工单号
    /// </summary>
    public string? ResolutionNote { get; private set; }

    /// <summary>
    /// 记录一个新发现的问题
    /// </summary>
    public static StorageIssue Found(StoredFileId fileId, StorageIssueKind kind, string? objectVersionId, DateTimeOffset now) =>
        new(StorageIssueId.New(), fileId, kind, objectVersionId, now);

    /// <summary>
    /// 标为已解决；已经解决的再来一次什么也不改
    /// </summary>
    public Result Resolve(string note, UserId by, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(note);

        if (Status == StorageIssueStatus.Resolved)
        {
            return Result.Success();
        }

        Status = StorageIssueStatus.Resolved;
        ResolvedAt = now;
        ResolvedBy = by;
        ResolutionNote = note;
        return Result.Success();
    }
}
