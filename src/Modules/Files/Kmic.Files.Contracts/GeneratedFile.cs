namespace Kmic.Files.Contracts;

/// <summary>
/// 已经写进存储、等待登记的生成文件
/// </summary>
/// <remarks>
/// 将此结果传入业务命令，并在事务中调用 <see cref="IGeneratedFiles.Register" /> 请求登记
/// </remarks>
/// <param name="FileId">文件 ID，登记完成之后用它关联或下载</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="ObjectVersionId">对象版本 ID，存储没有开启版本控制时为空</param>
/// <param name="Name">文件名</param>
/// <param name="FileType">文件类型编码</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
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
