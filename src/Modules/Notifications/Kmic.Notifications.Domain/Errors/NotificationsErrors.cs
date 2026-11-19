using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Errors;

/// <summary>
/// Notifications 模块的错误目录
/// </summary>
public static class NotificationsErrors
{
    /// <summary>
    /// 站内消息不存在，或者不是当前用户的
    /// </summary>
    public static readonly Error SiteMessageNotFound = Error.NotFound("Notifications.SiteMessageNotFound", "消息不存在");

    /// <summary>
    /// 站内消息链接不符合相对路径要求
    /// </summary>
    public static readonly Error SiteMessageLinkInvalid = Error.BusinessRule("Notifications.SiteMessageLinkInvalid", "链接须为以单个 / 开头的站内相对路径");

    /// <summary>
    /// 语法或渲染错误，具体原因见错误消息
    /// </summary>
    public static Error TemplateInvalid(string reason) => Error.BusinessRule("Notifications.TemplateInvalid", $"模板有误：{reason}");

    /// <summary>
    /// 通知模板未声明或不存在
    /// </summary>
    public static readonly Error TemplateNotFound = Error.NotFound("Notifications.TemplateNotFound", "通知模板不存在");

    /// <summary>
    /// 含密模板只能用含密请求发送，普通模板只能用普通请求发送
    /// </summary>
    public static readonly Error TemplateSecretMismatch = Error.BusinessRule("Notifications.TemplateSecretMismatch", "含密模板只能用含密请求发送，普通模板只能用普通请求发送");

    /// <summary>
    /// 模板已从代码中移除，不能修改
    /// </summary>
    public static readonly Error TemplateRetired = Error.BusinessRule("Notifications.TemplateRetired", "模板已从代码中移除，不能修改");

    /// <summary>
    /// 模板未声明指定渠道
    /// </summary>
    public static readonly Error TemplateChannelNotDeclared = Error.BusinessRule("Notifications.TemplateChannelNotDeclared", "模板未声明此渠道");

    /// <summary>
    /// 模板键已存在，并发同步由唯一索引保护
    /// </summary>
    public static readonly Error TemplateKeyDuplicated = Error.Conflict("Notifications.TemplateKeyDuplicated", "模板键已存在");

    /// <summary>
    /// 并发发布导致模板版本号冲突
    /// </summary>
    public static readonly Error TemplateVersionDuplicated = Error.Conflict("Notifications.TemplateVersionDuplicated", "模板版本号已被使用，请刷新后重试");

    /// <summary>
    /// 含密模板预览固定使用示例数据，不接受自定义数据
    /// </summary>
    public static readonly Error PreviewModelNotAllowed = Error.BusinessRule("Notifications.PreviewModelNotAllowed", "含密模板预览不接受自定义数据，请移除 model 参数");

    /// <summary>
    /// 仅失败且未放弃的投递可重试
    /// </summary>
    public static readonly Error DeliveryNotRetryable = Error.BusinessRule("Notifications.DeliveryNotRetryable", "仅失败且未放弃的投递可重试");

    /// <summary>
    /// 含密通知不能重试
    /// </summary>
    public static readonly Error DeliverySecretNotRetryable = Error.BusinessRule("Notifications.DeliverySecretNotRetryable", "含密通知失败时已清除内容，不能重试");

    /// <summary>
    /// 仅失败且未放弃的投递可放弃
    /// </summary>
    public static readonly Error DeliveryNotAbandonable = Error.BusinessRule("Notifications.DeliveryNotAbandonable", "仅失败且未放弃的投递可放弃");

    /// <summary>
    /// 通知发送请求或投递记录重复
    /// </summary>
    public static readonly Error DeliveryDuplicated = Error.Conflict("Notifications.DeliveryDuplicated", "通知记录已存在");

    /// <summary>
    /// 发送数据与模板声明的数据模型不符
    /// </summary>
    public static Error ModelInvalid(string templateKey) => Error.BusinessRule("Notifications.ModelInvalid", $"通知数据不符合模板 {templateKey} 声明的数据模型");

    /// <summary>
    /// 发送请求的记录不存在
    /// </summary>
    public static readonly Error NotificationNotFound = Error.NotFound("Notifications.NotificationNotFound", "通知记录不存在");

    /// <summary>
    /// 投递不存在
    /// </summary>
    public static readonly Error DeliveryNotFound = Error.NotFound("Notifications.DeliveryNotFound", "投递记录不存在");

    /// <summary>
    /// 捕获内容不存在或当前环境未启用捕获
    /// </summary>
    public static readonly Error CapturedUnavailable = Error.NotFound("Notifications.CapturedUnavailable", "捕获内容不存在或不可用");
}
