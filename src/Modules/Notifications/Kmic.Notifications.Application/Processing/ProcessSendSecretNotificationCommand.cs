using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>处理含一次性凭据的通知请求（高优先级、无附件，载荷不记录日志）</summary>
/// <param name="Message">请求</param>
/// <param name="SourceMessageId">来源消息的 ID</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">通知发起用户 ID</param>
[SystemOnly]
[NotAudited("发送请求、投递和站内消息已有专门记录")]
public sealed record ProcessSendSecretNotificationCommand(SendSecretNotification Message, Guid SourceMessageId, string SourceModule, Guid? InitiatorUserId) : ICommand;
