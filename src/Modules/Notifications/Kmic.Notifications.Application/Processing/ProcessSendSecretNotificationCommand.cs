using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理含一次性凭据的通知发送请求，使用高优先级且不带附件
/// </summary>
/// <remarks>
/// 载荷不写日志
/// </remarks>
/// <param name="Message">请求</param>
/// <param name="SourceMessageId">来源消息的 ID</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">通知发起用户 ID</param>
[SystemOnly]
[NotAudited("发送请求、投递和站内消息已有专门记录")]
public sealed record ProcessSendSecretNotificationCommand(SendSecretNotification Message, Guid SourceMessageId, string SourceModule, Guid? InitiatorUserId) : ICommand;
