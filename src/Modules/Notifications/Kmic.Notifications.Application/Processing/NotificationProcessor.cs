using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理一次发送请求：渲染内容，给每个收件人在每个渠道生成投递或站内消息
/// </summary>
/// <remarks>
/// 在 Notifications 自己的事务里完成，没有外部调用：渲染的结果随投递一起保存，投递由工作器之后再发出去。
/// 同一条来源消息只处理一次；同一个模板、收件人、去重键、渠道已经有投递的跳过。
/// 业务失败（模板不存在、模型不符）返回错误，消息进入死信，不是对用户的错误
/// </remarks>
public sealed class NotificationProcessor(
    NotificationTemplateCatalog catalog,
    ITemplateRenderer renderer,
    IUserContacts contacts,
    INotificationRepository notifications,
    IDeliveryRepository deliveries,
    ICapturedMessageRepository captured,
    ISiteMessageRepository siteMessages,
    IOptions<NotificationsCaptureOptions> capture,
    TimeProvider time)
{
    /// <summary>
    /// 处理一次发送请求
    /// </summary>
    /// <param name="request">发送请求</param>
    /// <param name="ct">取消令牌</param>
    public async Task<Result> ProcessAsync(NotificationRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var definition = catalog.Find(request.TemplateKey);
        if (definition is null)
        {
            return NotificationsErrors.TemplateNotFound;
        }

        if (definition.CarriesSecret != request.Secret)
        {
            return NotificationsErrors.TemplateSecretMismatch;
        }

        // 同一条来源消息只处理一次：重复投递的消息到这里什么也不做
        if (await notifications.ExistsForAsync(request.SourceMessageId, ct))
        {
            return Result.Success();
        }

        var model = ReadModel(definition, request.Model);
        if (model.IsFailure)
        {
            return model.Error;
        }

        var content = await RenderAsync(definition, model.Value, ct);
        if (content.IsFailure)
        {
            return content.Error;
        }

        var notification = Notification.Create(
            definition.Key, VersionsOf(definition), request.DedupKey, request.SourceMessageId, request.SourceModule, request.InitiatorUserId is { } initiator ? UserId.From(initiator) : null);
        notifications.Add(notification);
        var existing = await deliveries.FindExistingAsync(definition.Key, request.DedupKey, ct);
        var users = await contacts.GetAsync([.. request.To.Where(r => r.UserId is not null).Select(r => r.UserId!.Value).Distinct()], ct);
        var context = new FanOut(notification.Id, definition, request.DedupKey, content.Value, model.Value, request.HighPriority ? DeliveryPriority.High : DeliveryPriority.Normal, users, existing, time.GetUtcNow());
        foreach (var recipient in request.To)
        {
            Distribute(recipient, context);
        }

        return Result.Success();
    }

    private static Result<object> ReadModel(NotificationTemplateDefinition definition, JsonElement json)
    {
        try
        {
            return JsonSerializer.Deserialize(json.GetRawText(), definition.ModelType, NotificationModelJson.Options) is { } model
                ? model
                : NotificationsErrors.ModelInvalid(definition.Key);
        }
        catch (JsonException)
        {
            return NotificationsErrors.ModelInvalid(definition.Key);
        }
    }

    // 内容和收件人无关，渲染一次：主题与短信、站内消息是纯文本，邮件正文对变量的值转义
    private async Task<Result<RenderedContent>> RenderAsync(NotificationTemplateDefinition definition, object model, CancellationToken ct)
    {
        var subject = await renderer.RenderAsync(definition.DefaultSubject, model, RenderFormat.Text, ct);
        if (subject.IsFailure)
        {
            return subject.Error;
        }

        var html = await renderer.RenderAsync(definition.DefaultBody, model, RenderFormat.Html, ct);
        if (html.IsFailure)
        {
            return html.Error;
        }

        var text = await renderer.RenderAsync(definition.DefaultTextBody, model, RenderFormat.Text, ct);
        return text.IsFailure
            ? text.Error
            : new RenderedContent(subject.Value, html.Value, text.Value, HashOf(definition));
    }

    private static string HashOf(NotificationTemplateDefinition definition) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"{definition.Key}\n{definition.DefaultSubject}\n{definition.DefaultBody}\n{definition.DefaultTextBody}")));

    // 本次各渠道实际用的版本：现在只有代码里的默认内容，版本号都是 0
    private static string VersionsOf(NotificationTemplateDefinition definition) =>
        JsonSerializer.Serialize(Enum.GetValues<NotificationChannels>().Where(c => c != NotificationChannels.None && definition.Channels.HasFlag(c)).ToDictionary(c => c.ToString(), _ => 0, StringComparer.Ordinal));

    private void Distribute(Recipient recipient, FanOut context)
    {
        if (context.Definition.Channels.HasFlag(NotificationChannels.InApp) && recipient.UserId is { } userId && context.Users.TryGetValue(userId, out var contact) && contact.IsActive)
        {
            AddSiteMessage(UserId.From(userId), context);
        }

        if (context.Definition.Channels.HasFlag(NotificationChannels.Email))
        {
            AddDelivery(recipient, DeliveryChannel.Email, context);
        }

        if (context.Definition.Channels.HasFlag(NotificationChannels.Sms))
        {
            AddDelivery(recipient, DeliveryChannel.Sms, context);
        }
    }

    private void AddSiteMessage(UserId userId, FanOut context)
    {
        // 链接约定取模型上叫 Link 的字符串属性；不是站内地址就不带链接，不能因为一个链接让整条通知发不出去
        var link = context.ModelType.GetProperty("Link")?.GetValue(context.Model) as string;
        var created = SiteMessage.Create(userId, context.NotificationId, context.Content.Subject, context.Content.TextBody, link, context.Definition.Module, context.Now);
        if (created.IsFailure)
        {
            created = SiteMessage.Create(userId, context.NotificationId, context.Content.Subject, context.Content.TextBody, null, context.Definition.Module, context.Now);
        }

        siteMessages.Add(created.Value);
    }

    private void AddDelivery(Recipient recipient, DeliveryChannel channel, FanOut context)
    {
        var (userId, key, address, skip) = Resolve(recipient, channel, context);
        if (!context.Seen.Add($"{key}|{channel.Code}") || context.Existing.Contains($"{key}|{channel.Code}"))
        {
            return;
        }

        var role = DeliveryRole.FromCode(recipient.Role.ToString());
        var definition = context.Definition;
        if (skip is not null || address is null)
        {
            deliveries.Add(Delivery.Skip(context.NotificationId, channel, userId, key, role, context.Content.Subject, definition.Key, context.DedupKey, 0, context.Content.CodeHash, context.Priority, skip ?? DeliverySkipReason.NoAddress));
            return;
        }

        var body = channel == DeliveryChannel.Email ? context.Content.HtmlBody : context.Content.TextBody;
        var masked = channel == DeliveryChannel.Email ? AddressMasking.Email(address) : AddressMasking.Phone(address);
        var direct = userId is null ? address : null;
        if (capture.Value.Enabled)
        {
            var captureDelivery = Delivery.Capture(context.NotificationId, channel, userId, key, masked, direct, role, context.Content.Subject, body, definition.Key, context.DedupKey, 0, context.Content.CodeHash, context.Priority);
            deliveries.Add(captureDelivery);
            captured.Add(CapturedMessage.Create(captureDelivery.Id, channel, address, role, context.Content.Subject, body, context.Now));
            return;
        }

        deliveries.Add(Delivery.Queue(context.NotificationId, channel, userId, key, masked, direct, role, context.Content.Subject, body, definition.Key, context.DedupKey, 0, context.Content.CodeHash, context.Priority));
    }

    // 收件人在这个渠道的地址：用户向联系方式查，直接给的用其中的邮箱或手机号；没有地址或用户不可用时给出跳过的原因
    private static (UserId? UserId, string Key, string? Address, DeliverySkipReason? Skip) Resolve(Recipient recipient, DeliveryChannel channel, FanOut context)
    {
        var direct = channel == DeliveryChannel.Email ? recipient.Email : recipient.Phone;
        if (recipient.UserId is not { } id)
        {
            // 系统外的收件人：这个渠道给了地址就用它；没给就按他给的另一个地址（都没有就是占位）记一条跳过的投递
            if (string.IsNullOrWhiteSpace(direct))
            {
                return (null, AddressMasking.KeyOfAddress(recipient.Email ?? recipient.Phone ?? "-"), null, DeliverySkipReason.NoAddress);
            }

            return (null, AddressMasking.KeyOfAddress(direct), direct.Trim(), null);
        }

        var key = AddressMasking.KeyOfUser(id);
        if (!context.Users.TryGetValue(id, out var contact))
        {
            return (UserId.From(id), key, null, DeliverySkipReason.UnknownUser);
        }

        if (!contact.IsActive)
        {
            return (UserId.From(id), key, null, DeliverySkipReason.UserDisabled);
        }

        var address = channel == DeliveryChannel.Email ? contact.Email : contact.Mobile;
        if (string.IsNullOrWhiteSpace(address))
        {
            return (UserId.From(id), key, null, DeliverySkipReason.NoAddress);
        }

        return (UserId.From(id), key, address.Trim(), null);
    }

    // 处理一次请求用到的共享状态：已经渲染好的内容、联系方式、已有的投递、本次已经生成的投递键
    private sealed class FanOut(
        NotificationId notificationId,
        NotificationTemplateDefinition definition,
        string dedupKey,
        RenderedContent content,
        object model,
        DeliveryPriority priority,
        IReadOnlyDictionary<Guid, UserContact> users,
        IReadOnlySet<string> existing,
        DateTimeOffset now)
    {
        public NotificationId NotificationId => notificationId;

        public NotificationTemplateDefinition Definition => definition;

        public string DedupKey => dedupKey;

        public RenderedContent Content => content;

        public object Model => model;

        public Type ModelType => model.GetType();

        public DeliveryPriority Priority => priority;

        public IReadOnlyDictionary<Guid, UserContact> Users => users;

        public IReadOnlySet<string> Existing => existing;

        public DateTimeOffset Now => now;

        public HashSet<string> Seen { get; } = new(StringComparer.Ordinal);
    }
}
