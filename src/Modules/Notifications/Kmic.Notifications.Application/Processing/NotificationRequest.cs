using System.Text.Json;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 普通通知和含密通知共用的内部发送请求
/// </summary>
/// <param name="TemplateKey">模板键</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">业务去重键</param>
/// <param name="Secret">是否为含密通知</param>
/// <param name="HighPriority">是否高优先级</param>
/// <param name="SourceMessageId">来源消息 ID</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">通知发起用户 ID</param>
/// <param name="AttachmentFileIds">邮件附件的文件 ID；没有附件为空</param>
public sealed record NotificationRequest(
    string TemplateKey,
    IReadOnlyList<Recipient> To,
    JsonElement Model,
    string DedupKey,
    bool Secret,
    bool HighPriority,
    Guid SourceMessageId,
    string SourceModule,
    Guid? InitiatorUserId,
    IReadOnlyList<Guid>? AttachmentFileIds = null);
