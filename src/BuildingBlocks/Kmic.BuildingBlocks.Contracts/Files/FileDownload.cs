namespace Kmic.BuildingBlocks.Contracts.Files;

/// <summary>文件下载信息（返回此类型的查询必须标注 [AuditedRead]）。</summary>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">存储对象键</param>
/// <param name="Name">下载文件名</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">文件字节数</param>
/// <param name="Sha256">SHA-256 摘要</param>
/// <param name="FileType">文件类型编码</param>
public sealed record FileDownload(Guid FileId, string StorageKey, string Name, string ContentType, long Size, string Sha256, string FileType);
