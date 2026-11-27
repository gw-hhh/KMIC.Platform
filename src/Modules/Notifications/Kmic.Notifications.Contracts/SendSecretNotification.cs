using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>敏感通知发送请求，仅限携带 5 分钟内用后即失效的凭据（如验证码）</summary>
/// <param name="TemplateKey">模板键</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">业务去重键，须包含本次凭据标识</param>
[IntegrationMessage("Notifications.SendSecretNotification", CarriesSecret = true, SecretLifetimeSeconds = SendSecretNotification.LifetimeSeconds)]
public sealed record SendSecretNotification(string TemplateKey, IReadOnlyList<Recipient> To, JsonElement Model, string DedupKey) : IIntegrationRequest
{
    /// <summary>敏感投递有效期（秒）</summary>
    public const int LifetimeSeconds = 300;

    /// <summary>按敏感模板模型声明构造发送请求</summary>
    public static SendSecretNotification For<TModel>(TModel model, IReadOnlyList<Recipient> to, string dedupKey)
        where TModel : INotificationModel<TModel>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(dedupKey);

        var template = typeof(TModel).GetCustomAttribute<NotificationTemplateAttribute>()
            ?? throw new ArgumentException($"{typeof(TModel).Name} 未声明 [NotificationTemplate]。", nameof(model));
        return template.CarriesSecret
            ? new SendSecretNotification(template.Key, to, NotificationModelJson.Serialize(model), dedupKey)
            : throw new ArgumentException($"{template.Key} 不是敏感模板，须使用 SendNotification 发送。", nameof(model));
    }
}
