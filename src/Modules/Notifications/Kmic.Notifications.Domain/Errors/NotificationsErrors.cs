using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Errors;

public static class NotificationsErrors
{
    /// <summary>站内消息不存在或不属于当前用户</summary>
    public static readonly Error SiteMessageNotFound = Error.NotFound("Notifications.SiteMessageNotFound", "消息不存在");

    public static readonly Error SiteMessageLinkInvalid = Error.BusinessRule("Notifications.SiteMessageLinkInvalid", "链接须为以单个 / 开头的站内相对路径");

    /// <summary>模板语法或渲染失败，具体原因见 <paramref name="reason" /></summary>
    public static Error TemplateInvalid(string reason) => Error.BusinessRule("Notifications.TemplateInvalid", $"模板有误：{reason}");

    public static readonly Error TemplateNotFound = Error.NotFound("Notifications.TemplateNotFound", "通知模板不存在");

    public static readonly Error TemplateSecretMismatch = Error.BusinessRule("Notifications.TemplateSecretMismatch", "敏感模板只能用敏感通知方式发送，普通模板只能用普通通知方式发送");

    public static readonly Error TemplateRetired = Error.BusinessRule("Notifications.TemplateRetired", "模板已从代码中移除，不能修改");

    public static readonly Error TemplateChannelNotDeclared = Error.BusinessRule("Notifications.TemplateChannelNotDeclared", "模板未声明此渠道");

    /// <summary>模板键已存在（并发同步由唯一索引兜底）</summary>
    public static readonly Error TemplateKeyDuplicated = Error.Conflict("Notifications.TemplateKeyDuplicated", "模板键已存在");

    /// <summary>并发发布导致版本号冲突</summary>
    public static readonly Error TemplateVersionDuplicated = Error.Conflict("Notifications.TemplateVersionDuplicated", "模板版本号已被使用，请刷新后重试");

    /// <summary>敏感模板预览仅使用示例数据，不接受自定义数据</summary>
    public static readonly Error PreviewModelNotAllowed = Error.BusinessRule("Notifications.PreviewModelNotAllowed", "敏感模板预览仅使用示例数据，不支持自定义数据");

    public static readonly Error DeliveryNotRetryable = Error.BusinessRule("Notifications.DeliveryNotRetryable", "仅失败且未放弃的投递可重试");

    public static readonly Error DeliverySecretNotRetryable = Error.BusinessRule("Notifications.DeliverySecretNotRetryable", "敏感通知失败时已清除内容，不能重试");

    public static readonly Error DeliveryNotAbandonable = Error.BusinessRule("Notifications.DeliveryNotAbandonable", "仅失败且未放弃的投递可放弃");

    public static readonly Error DeliveryDuplicated = Error.Conflict("Notifications.DeliveryDuplicated", "通知记录已存在");

    /// <summary>发送数据与模板声明的模型不符</summary>
    public static Error ModelInvalid(string templateKey) => Error.BusinessRule("Notifications.ModelInvalid", $"通知数据不符合模板 {templateKey} 声明的数据模型");

    public static readonly Error NotificationNotFound = Error.NotFound("Notifications.NotificationNotFound", "通知记录不存在");

    public static readonly Error DeliveryNotFound = Error.NotFound("Notifications.DeliveryNotFound", "投递记录不存在");

    /// <summary>捕获内容不存在或当前环境未开启捕获</summary>
    public static readonly Error CapturedUnavailable = Error.NotFound("Notifications.CapturedUnavailable", "捕获内容不存在或不可用");
}
