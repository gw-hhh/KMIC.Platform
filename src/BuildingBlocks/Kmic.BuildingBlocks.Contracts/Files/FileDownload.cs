namespace Kmic.BuildingBlocks.Contracts.Files;

/// <summary>
/// 一次文件下载：业务的下载查询授权、确认关联之后组装出来，端点用 FileResults.Stream 把它写成响应
/// </summary>
/// <remarks>
/// 返回它的查询必须声明 [AuditedRead]（启动时校验）：每次下载都留审计记录。
/// 内容类型用 Files 识别出的值，不用上传时客户端声明的
/// </remarks>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="Name">下载时的文件名</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256，响应的 ETag</param>
/// <param name="FileType">文件类型编码，决定能不能在浏览器里直接打开</param>
public sealed record FileDownload(Guid FileId, string StorageKey, string Name, string ContentType, long Size, string Sha256, string FileType);
