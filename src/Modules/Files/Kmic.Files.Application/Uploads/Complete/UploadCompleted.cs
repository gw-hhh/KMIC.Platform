namespace Kmic.Files.Application.Uploads.Complete;

/// <summary>
/// 上传完成的回执：前端拿文件 ID 去关联，其余是显示用的
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="Name">文件名</param>
/// <param name="Size">字节数</param>
/// <param name="Type">文件类型编码</param>
/// <param name="ExpiresAt">没有被业务关联的话，到这个时间就会清理</param>
public sealed record UploadCompleted(Guid FileId, string Name, long Size, string Type, DateTimeOffset ExpiresAt);
