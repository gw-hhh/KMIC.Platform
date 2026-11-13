using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 文件的详情：元数据、状态和引用，不含存储键和内容
/// </summary>
/// <param name="Id">文件</param>
/// <param name="Version">并发版本号，修改时作为 expectedVersion 带回</param>
/// <param name="Name">显示用的文件名</param>
/// <param name="FileType">文件类型编码</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256</param>
/// <param name="Profile">上传档案</param>
/// <param name="UploadedBy">上传的人</param>
/// <param name="UploadedAt">上传开始的时间</param>
/// <param name="Status">状态</param>
/// <param name="ExpiresAt">成为清理候选的时间</param>
/// <param name="RejectReason">被拒绝的原因</param>
/// <param name="PurgedAt">彻底删除的时间</param>
/// <param name="PurgedBy">彻底删除的人</param>
/// <param name="PurgeReason">彻底删除的原因</param>
/// <param name="ObjectDeletedAt">对象在存储里被删除的时间，彻底删除的文件才有</param>
/// <param name="HistoryPurgedAt">运维登记彻底删除历史版本的时间，空表示历史版本可能仍在</param>
/// <param name="HistoryPurgeNote">彻底删除历史版本的执行记录</param>
/// <param name="References">引用它的业务对象</param>
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
    IReadOnlyList<FileReferenceItem> References);
