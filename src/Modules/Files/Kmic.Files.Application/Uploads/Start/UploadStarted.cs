namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 登记好的上传：往哪里写、写完登记到哪个文件
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">写入存储用的键</param>
/// <param name="FileName">整理后的文件名</param>
public sealed record UploadStarted(Guid FileId, string StorageKey, string FileName);
