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

    /// <summary>
    /// 模板有误：语法错误、渲染超时、输出过大，原因写在消息里
    /// </summary>
    /// <param name="reason">原因，包含出错的位置</param>
    public static Error TemplateInvalid(string reason) => Error.BusinessRule("Notifications.TemplateInvalid", $"模板有误：{reason}");
}
