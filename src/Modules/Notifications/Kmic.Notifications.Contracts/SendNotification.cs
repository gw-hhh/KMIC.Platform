using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>发送通知请求（敏感凭据模板请改用 <see cref="SendSecretNotification" />）</summary>
/// <param name="TemplateKey">模板键</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">业务去重键</param>
/// <param name="AttachmentFileIds">附件文件 ID 列表</param>
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
    /// <summary>按模板模型声明构造发送请求</summary>
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
            ?? throw new ArgumentException($"{typeof(TModel).Name} 未声明 [NotificationTemplate]。", nameof(model));
        return template.CarriesSecret
            ? throw new ArgumentException($"{template.Key} 是敏感模板，须使用 SendSecretNotification 发送。", nameof(model))
            : new SendNotification(template.Key, to, NotificationModelJson.Serialize(model), dedupKey, attachmentFileIds, priority);
    }
}
