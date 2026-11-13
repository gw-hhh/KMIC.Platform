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

    /// <summary>
    /// 模板键没有声明过，发送请求提到了一个不存在的模板
    /// </summary>
    public static readonly Error TemplateNotFound = Error.NotFound("Notifications.TemplateNotFound", "通知模板不存在");

    /// <summary>
    /// 含密模板只能用含密请求发送，普通模板只能用普通请求发送
    /// </summary>
    public static readonly Error TemplateSecretMismatch = Error.BusinessRule("Notifications.TemplateSecretMismatch", "含密模板只能用含密通知发送，普通模板不能用含密通知发送");

    /// <summary>
    /// 同一个模板、收件人、去重键、渠道已经有一条投递
    /// </summary>
    public static readonly Error DeliveryDuplicated = Error.Conflict("Notifications.DeliveryDuplicated", "这条通知已经发给过这个收件人");

    /// <summary>
    /// 发送请求里的数据和模板声明的数据模型对不上
    /// </summary>
    /// <param name="templateKey">模板键</param>
    public static Error ModelInvalid(string templateKey) => Error.BusinessRule("Notifications.ModelInvalid", $"通知数据和模板 {templateKey} 声明的数据模型不符");

    /// <summary>
    /// 发送请求的记录不存在
    /// </summary>
    public static readonly Error NotificationNotFound = Error.NotFound("Notifications.NotificationNotFound", "通知记录不存在");

    /// <summary>
    /// 投递不存在
    /// </summary>
    public static readonly Error DeliveryNotFound = Error.NotFound("Notifications.DeliveryNotFound", "投递记录不存在");

    /// <summary>
    /// 捕获的内容不存在，或者当前环境不捕获（生产环境）
    /// </summary>
    public static readonly Error CapturedUnavailable = Error.NotFound("Notifications.CapturedUnavailable", "没有这条捕获的内容");
}
