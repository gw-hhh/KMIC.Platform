using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 存储问题列表里的一行
/// </summary>
/// <param name="Id">存储问题</param>
/// <param name="FileId">出问题的文件</param>
/// <param name="FileName">文件名，文件的记录已经清理时为空</param>
/// <param name="Kind">种类</param>
/// <param name="ObjectVersionId">发现时文件记录的对象版本，还原时按它找</param>
/// <param name="FoundAt">发现的时间</param>
/// <param name="Status">状态</param>
/// <param name="ResolvedAt">标为已解决的时间</param>
/// <param name="ResolvedBy">标为已解决的人</param>
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
