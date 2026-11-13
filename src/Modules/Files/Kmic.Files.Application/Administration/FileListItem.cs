using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 文件列表里的一行，不含存储键
/// </summary>
/// <param name="Id">文件</param>
/// <param name="Name">显示用的文件名</param>
/// <param name="FileType">文件类型编码，上传没写完的没有</param>
/// <param name="Size">字节数，上传没写完的没有</param>
/// <param name="Profile">上传档案</param>
/// <param name="UploadedBy">上传的人，系统生成的文件没有</param>
/// <param name="UploadedAt">上传开始的时间</param>
/// <param name="Status">状态</param>
/// <param name="ExpiresAt">成为清理候选的时间</param>
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
