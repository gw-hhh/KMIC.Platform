using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 登记业务模块生成的文件
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="ObjectVersionId">对象版本 ID，存储没有开启版本控制时为空</param>
/// <param name="OriginalName">显示文件名</param>
/// <param name="FileType">文件类型编码</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Sha256">内容的 SHA-256</param>
/// <param name="Source">生成方</param>
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
