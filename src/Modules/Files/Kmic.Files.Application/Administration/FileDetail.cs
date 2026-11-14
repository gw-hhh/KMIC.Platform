using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 文件详情，包含元数据、状态、引用和历史，不含存储键和内容
/// </summary>
/// <param name="Id">文件 ID</param>
/// <param name="Version">并发版本号，修改时通过 expectedVersion 提交</param>
/// <param name="Name">显示文件名</param>
/// <param name="FileType">文件类型编码</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256</param>
/// <param name="Profile">上传配置名称；生成文件为 Generated:来源</param>
/// <param name="UploadedBy">上传人或生成请求人 ID，可为空</param>
/// <param name="UploadedAt">上传开始时间；生成文件为登记时间</param>
/// <param name="Status">状态</param>
/// <param name="ExpiresAt">清理到期时间</param>
/// <param name="RejectReason">拒绝原因</param>
/// <param name="PurgedAt">彻底删除标记时间</param>
/// <param name="PurgedBy">彻底删除操作人 ID</param>
/// <param name="PurgeReason">彻底删除原因</param>
/// <param name="ObjectDeletedAt">存储对象删除时间，仅已彻底删除的文件记录</param>
/// <param name="HistoryPurgedAt">历史版本清除登记时间，为空时历史版本可能仍存在</param>
/// <param name="HistoryPurgeNote">历史版本清除记录</param>
/// <param name="References">引用它的业务对象</param>
/// <param name="History">状态流转记录，按时间从早到晚</param>
public sealed record FileDetail(
    StoredFileId Id,
    long Version,
    string Name,
    string? FileType,
    string? ContentType,
    long? Size,
    string? Sha256,
    string Profile,
    UserId? UploadedBy,
    DateTimeOffset UploadedAt,
    StoredFileStatus Status,
    DateTimeOffset? ExpiresAt,
    string? RejectReason,
    DateTimeOffset? PurgedAt,
    UserId? PurgedBy,
    string? PurgeReason,
    DateTimeOffset? ObjectDeletedAt,
    DateTimeOffset? HistoryPurgedAt,
    string? HistoryPurgeNote,
    IReadOnlyList<FileReferenceItem> References,
    IReadOnlyList<StateHistoryItem> History);
