using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Contracts;

/// <summary>
/// 请求发送一条含一次性短时凭据的通知，如短信验证码
/// </summary>
/// <remarks>
/// 消息载荷不能放凭据，这是唯一的例外：凭据只存哈希、用后即废，有效期不超过 5 分钟，所以载荷里的凭据过期就没有价值。
/// 这类消息的载荷不进日志，不带附件，投递完成后 Notifications 清空渲染出的正文
/// </remarks>
/// <param name="TemplateKey">模板键，必须是含密模板</param>
/// <param name="To">收件人</param>
/// <param name="Model">模板数据</param>
/// <param name="DedupKey">去重键，要包含凭据的标识，同一手机号的两次验证码不能被合并</param>
[IntegrationMessage("Notifications.SendSecretNotification", CarriesSecret = true, SecretLifetimeSeconds = SendSecretNotification.LifetimeSeconds)]
public sealed record SendSecretNotification(string TemplateKey, IReadOnlyList<Recipient> To, JsonElement Model, string DedupKey) : IIntegrationRequest
{
    /// <summary>
    /// 凭据有效期，秒
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
            ?? throw new ArgumentException($"{typeof(TModel).Name} 没有标 [NotificationTemplate]。", nameof(model));
        return template.CarriesSecret
            ? new SendSecretNotification(template.Key, to, NotificationModelJson.Serialize(model), dedupKey)
            : throw new ArgumentException($"{template.Key} 不是含密模板，要用 SendNotification 发送。", nameof(model));
    }
}
