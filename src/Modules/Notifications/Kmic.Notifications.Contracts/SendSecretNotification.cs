using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>
/// 请求发送一条含一次性短时凭据的通知，如短信验证码
/// </summary>
/// <remarks>
/// 只能带用后即失效、5 分钟内有效的凭据
/// </remarks>
/// <param name="TemplateKey">模板键</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">去重键须包含本次凭据的标识</param>
[IntegrationMessage("Notifications.SendSecretNotification", CarriesSecret = true, SecretLifetimeSeconds = SendSecretNotification.LifetimeSeconds)]
public sealed record SendSecretNotification(string TemplateKey, IReadOnlyList<Recipient> To, JsonElement Model, string DedupKey) : IIntegrationRequest
{
    /// <summary>
    /// 含密投递有效期（秒）
    /// </summary>
    public const int LifetimeSeconds = 300;

    /// <summary>
    /// 按模型上声明的含密模板构造发送请求
    /// </summary>
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
            : throw new ArgumentException($"{template.Key} 不是含密模板，须使用 SendNotification 发送。", nameof(model));
    }
}
