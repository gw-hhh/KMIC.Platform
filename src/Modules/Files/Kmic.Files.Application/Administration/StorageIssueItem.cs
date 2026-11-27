using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>存储问题列表项</summary>
/// <param name="Id">存储问题 ID</param>
/// <param name="FileId">文件 ID</param>
/// <param name="FileName">文件名，文件记录不存在时为空</param>
/// <param name="Kind">问题类型</param>
/// <param name="ObjectVersionId">发现时记录的对象版本</param>
/// <param name="FoundAt">发现时间</param>
/// <param name="Status">状态</param>
/// <param name="ResolvedAt">标记已解决的时间</param>
/// <param name="ResolvedBy">标记已解决的用户 ID</param>
/// <param name="ResolutionNote">处理说明</param>
/// <param name="Version">并发版本号</param>
public sealed record StorageIssueItem(
    StorageIssueId Id,
    StoredFileId FileId,
    string? FileName,
    StorageIssueKind Kind,
    string? ObjectVersionId,
    DateTimeOffset FoundAt,
    StorageIssueStatus Status,
    DateTimeOffset? ResolvedAt,
    UserId? ResolvedBy,
    string? ResolutionNote,
    long Version);
