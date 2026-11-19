namespace Kmic.BuildingBlocks.Contracts.Files;

/// <summary>
/// 一次文件下载
/// </summary>
/// <remarks>
/// 返回它的查询必须声明 [AuditedRead]
/// </remarks>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="Name">下载时的文件名</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256</param>
/// <param name="FileType">文件类型编码</param>
public sealed record FileDownload(Guid FileId, string StorageKey, string Name, string ContentType, long Size, string Sha256, string FileType);
