using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理普通通知发送请求，生成投递记录或站内消息
/// </summary>
/// <param name="Message">请求</param>
/// <param name="SourceMessageId">来源消息的 ID</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">通知发起用户 ID</param>
[SystemOnly]
[NotAudited("发送请求、投递和站内消息已有专门记录")]
public sealed record ProcessSendNotificationCommand(SendNotification Message, Guid SourceMessageId, string SourceModule, Guid? InitiatorUserId) : ICommand;
