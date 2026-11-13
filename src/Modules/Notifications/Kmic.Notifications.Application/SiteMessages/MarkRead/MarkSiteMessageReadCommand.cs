using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 把我的一条站内消息标为已读
/// </summary>
/// <remarks>
/// 已经读过的再标一次什么也不改。别人的消息和不存在的消息一样，返回找不到
/// </remarks>
/// <param name="Id">消息</param>
[AllowAuthenticated]
[NotAudited("个人的已读状态，不是业务数据")]
public sealed record MarkSiteMessageReadCommand(SiteMessageId Id) : ICommand;
