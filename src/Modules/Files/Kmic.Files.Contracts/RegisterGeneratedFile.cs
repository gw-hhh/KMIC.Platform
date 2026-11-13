using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 登记一个模块自己生成的文件
/// </summary>
/// <remarks>
/// 导出作业的结果文件、系统生成的 PDF 等：生成方先把对象写进存储，再用这条请求型消息登记，
/// 与作业完成同一个事务进 Outbox。作业失败消息就不发，留下的对象由对账任务清理
/// </remarks>
/// <param name="FileId">文件 ID，生成方预先生成</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="ObjectVersionId">对象版本 ID，存储没有开启版本控制时为空</param>
/// <param name="OriginalName">显示用的文件名</param>
/// <param name="FileType">文件类型编码，见框架的 FileType</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
/// <param name="Source">生成方，如 Sample.ExportSpecimens，登记后上传档案是 Generated:来源</param>
/// <param name="RequestedBy">发起生成的用户；系统生成的没有</param>
/// <param name="ExpiresAt">到这个时间还没有被关联就清理</param>
[IntegrationMessage("Files.RegisterGeneratedFile")]
[MessageSubjects(nameof(FileId))]
public sealed record RegisterGeneratedFile(
    Guid FileId,
    string StorageKey,
    string? ObjectVersionId,
    string OriginalName,
    string FileType,
    string ContentType,
    long Size,
    string Sha256,
    string Source,
    Guid? RequestedBy,
    DateTimeOffset ExpiresAt) : IIntegrationRequest;
