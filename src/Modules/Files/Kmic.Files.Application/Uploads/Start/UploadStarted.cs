namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 上传登记结果，包含文件 ID、存储键和清理后的文件名
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">写入存储用的键</param>
/// <param name="FileName">清理后的文件名</param>
public sealed record UploadStarted(Guid FileId, string StorageKey, string FileName);
