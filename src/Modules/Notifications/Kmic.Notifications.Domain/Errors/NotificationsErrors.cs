using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Errors;

/// <summary>
/// Notifications 模块的错误目录
/// </summary>
/// <remarks>
/// 错误码写成“Notifications.错误名”，按聚合分文件时拆成多个 *Errors 静态类
/// </remarks>
public static class NotificationsErrors
{
    /// <summary>
    /// 站内消息不存在，或者不是当前用户的
    /// </summary>
    public static readonly Error SiteMessageNotFound = Error.NotFound("Notifications.SiteMessageNotFound", "消息不存在");

    /// <summary>
    /// 站内消息的链接不是站内地址
    /// </summary>
    public static readonly Error SiteMessageLinkInvalid = Error.BusinessRule("Notifications.SiteMessageLinkInvalid", "站内消息的链接只能是站内地址");
}
