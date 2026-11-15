using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 文件列表项，不含存储键
/// </summary>
/// <param name="Id">文件 ID</param>
/// <param name="Name">显示文件名</param>
/// <param name="FileType">文件类型编码，未登记写入结果时为空</param>
/// <param name="Size">文件大小（字节），未登记写入结果时为空</param>
/// <param name="Profile">上传配置标识；生成文件为 Generated:来源</param>
/// <param name="UploadedBy">上传人或生成请求人 ID，可为空</param>
/// <param name="UploadedAt">文件登记时间</param>
/// <param name="Status">状态</param>
/// <param name="ExpiresAt">清理到期时间</param>
/// <param name="Version">并发版本号</param>
public sealed record FileListItem(
    StoredFileId Id,
    string Name,
    string? FileType,
    long? Size,
    string Profile,
    UserId? UploadedBy,
    DateTimeOffset UploadedAt,
    StoredFileStatus Status,
    DateTimeOffset? ExpiresAt,
    long Version);
