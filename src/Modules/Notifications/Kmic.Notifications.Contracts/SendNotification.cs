using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>
/// 请求发送一条通知
/// </summary>
/// <remarks>
/// 请求型消息，随业务事务写入 Outbox，由 Notifications 去重、渲染、投递；业务回滚则不发。
/// 内容含验证码这类凭据的模板不能用它，改用 <see cref="SendSecretNotification" />。
/// 用 <see cref="For{TModel}" /> 构造，数据模型由框架序列化，和模板声明保持一致
/// </remarks>
/// <param name="TemplateKey">模板键，如 Sample.Approved</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据，结构由模板声明决定</param>
/// <param name="DedupKey">业务去重键：同一模板、同一收件人、同一键只发一次，如 Sample.Approved:{单据 ID}:{修订号}</param>
/// <param name="AttachmentFileIds">附件的文件 ID，投递时读取内容</param>
/// <param name="Priority">优先级</param>
[IntegrationMessage("Notifications.SendNotification")]
public sealed record SendNotification(
    string TemplateKey,
    IReadOnlyList<Recipient> To,
    JsonElement Model,
    string DedupKey,
    IReadOnlyList<Guid>? AttachmentFileIds = null,
    NotificationPriority Priority = NotificationPriority.Normal) : IIntegrationRequest
{
    /// <summary>
    /// 按模型上声明的模板构造发送请求
    /// </summary>
    public static SendNotification For<TModel>(
        TModel model,
        IReadOnlyList<Recipient> to,
        string dedupKey,
        IReadOnlyList<Guid>? attachmentFileIds = null,
        NotificationPriority priority = NotificationPriority.Normal)
        where TModel : INotificationModel<TModel>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(dedupKey);

        var template = typeof(TModel).GetCustomAttribute<NotificationTemplateAttribute>()
            ?? throw new ArgumentException($"{typeof(TModel).Name} 没有标 [NotificationTemplate]。", nameof(model));
        return template.CarriesSecret
            ? throw new ArgumentException($"{template.Key} 是含密模板，要用 SendSecretNotification 发送。", nameof(model))
            : new SendNotification(template.Key, to, NotificationModelJson.Serialize(model), dedupKey, attachmentFileIds, priority);
    }
}
