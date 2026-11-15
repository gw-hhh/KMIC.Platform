using System.Text.Json;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 普通通知和含密通知共用的内部发送请求
/// </summary>
/// <remarks>
/// DedupKey 用于邮件和短信去重，SourceMessageId 用于避免重复处理同一来源消息
/// </remarks>
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
