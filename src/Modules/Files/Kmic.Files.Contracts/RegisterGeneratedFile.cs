using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 登记业务模块生成的文件
/// </summary>
/// <remarks>
/// 用于导出结果、系统生成的 PDF 等。先写入对象存储，再在业务事务中将登记消息写入 Outbox。
/// 事务回滚时不发送消息，遗留对象由对账任务清理
/// </remarks>
/// <param name="FileId">文件 ID，生成方预先生成</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="ObjectVersionId">对象版本 ID，存储没有开启版本控制时为空</param>
/// <param name="OriginalName">显示文件名</param>
/// <param name="FileType">文件类型编码，见框架的 FileType</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
/// <param name="Source">生成方，如 Sample.ExportSpecimens；登记后的 Profile 为 Generated:来源</param>
/// <param name="RequestedBy">发起生成的用户 ID，无发起用户时为空</param>
/// <param name="ExpiresAt">未关联文件的清理到期时间</param>
[IntegrationMessage(RegisterGeneratedFile.MessageName)]
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
    DateTimeOffset ExpiresAt) : IIntegrationRequest
{
    /// <summary>
    /// 消息类型名，供对账任务在删除未登记对象前探查未完成的登记请求
    /// </summary>
    public const string MessageName = "Files.RegisterGeneratedFile";
}
