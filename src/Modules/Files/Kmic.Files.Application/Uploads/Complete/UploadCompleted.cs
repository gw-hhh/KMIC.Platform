namespace Kmic.Files.Application.Uploads.Complete;

/// <summary>上传完成结果，文件 ID 用于业务关联，其余信息用于显示</summary>
/// <param name="FileId">文件 ID</param>
/// <param name="Name">文件名</param>
/// <param name="Size">字节数</param>
/// <param name="Type">文件类型编码</param>
/// <param name="ExpiresAt">未关联文件的清理到期时间</param>
public sealed record UploadCompleted(Guid FileId, string Name, long Size, string Type, DateTimeOffset ExpiresAt);
