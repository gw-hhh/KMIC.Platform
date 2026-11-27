namespace Kmic.Files.Contracts;

/// <summary>已写入对象存储、待登记的生成文件</summary>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">对象存储键</param>
/// <param name="ObjectVersionId">对象版本 ID，存储未开启版本控制时为空</param>
/// <param name="Name">文件名</param>
/// <param name="FileType">文件类型编码</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容 SHA-256</param>
/// <param name="Source">生成方</param>
/// <param name="RequestedBy">生成请求人 ID，可为空</param>
/// <param name="ExpiresAt">未关联文件的清理到期时间</param>
public sealed record GeneratedFile(
    Guid FileId,
    string StorageKey,
    string? ObjectVersionId,
    string Name,
    string FileType,
    string ContentType,
    long Size,
    string Sha256,
    string Source,
    Guid? RequestedBy,
    DateTimeOffset ExpiresAt);
