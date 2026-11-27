using System.Text.Json;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>普通通知与敏感通知共用的内部发送请求</summary>
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
