using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理一条发送含密通知的请求，如短信验证码：高优先级，不带附件
/// </summary>
/// <remarks>
/// 订阅 Notifications.SendSecretNotification。请求的载荷里有一次性凭据，不写日志
/// </remarks>
/// <param name="Message">请求</param>
/// <param name="SourceMessageId">来源消息的 ID，同一条只处理一次</param>
/// <param name="SourceModule">发出请求的模块</param>
/// <param name="InitiatorUserId">发起这件事的用户</param>
[SystemOnly]
[NotAudited("通知记录即投递记录")]
public sealed record ProcessSendSecretNotificationCommand(SendSecretNotification Message, Guid SourceMessageId, string SourceModule, Guid? InitiatorUserId) : ICommand;
