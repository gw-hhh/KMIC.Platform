using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理一条发送通知的请求：渲染、给每个收件人生成投递或站内消息
/// </summary>
/// <remarks>
/// 订阅 Notifications.SendNotification，以系统身份在 Worker 上执行。业务失败（模板不存在、数据对不上）进入死信，
/// 不是对用户的错误
/// </remarks>
/// <param name="Message">请求</param>
/// <param name="SourceMessageId">来源消息的 ID，同一条只处理一次</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">发起这件事的用户</param>
[SystemOnly]
[NotAudited("通知记录即投递记录")]
public sealed record ProcessSendNotificationCommand(SendNotification Message, Guid SourceMessageId, string SourceModule, Guid? InitiatorUserId) : ICommand;
