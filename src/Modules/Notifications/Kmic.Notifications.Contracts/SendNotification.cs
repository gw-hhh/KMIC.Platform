using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>
/// 请求发送一条通知
/// </summary>
/// <remarks>
/// 含验证码的模板改用 <see cref="SendSecretNotification" />
/// </remarks>
/// <param name="TemplateKey">模板键</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">业务去重键</param>
/// <param name="AttachmentFileIds">附件的文件 ID</param>
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
            ?? throw new ArgumentException($"{typeof(TModel).Name} 未声明 [NotificationTemplate]。", nameof(model));
        return template.CarriesSecret
            ? throw new ArgumentException($"{template.Key} 是含密模板，须使用 SendSecretNotification 发送。", nameof(model))
            : new SendNotification(template.Key, to, NotificationModelJson.Serialize(model), dedupKey, attachmentFileIds, priority);
    }
}
