using System.Text.Json;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Application.Templates;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理一次发送请求：渲染内容，给每个收件人在每个渠道生成投递或站内消息
/// </summary>
/// <remarks>
/// 在 Notifications 自己的事务里完成，没有外部调用：渲染的结果随投递一起保存，投递由工作器之后再发出去。
/// 每个渠道用它当前生效的内容：发布过版本的用发布的版本，否则用代码里的默认内容。
/// 同一条来源消息只处理一次；同一个模板、收件人、去重键、渠道已经有投递的跳过。
/// 业务失败（模板不存在、模型不符）返回错误，消息进入死信，不是对用户的错误
/// </remarks>
public sealed class NotificationProcessor(
    NotificationTemplateCatalog catalog,
    ITemplateRenderer renderer,
    ITemplateRepository templates,
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

        var model = TemplateModels.Read(definition, request.Model);
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
            definition.Key, VersionsOf(content.Value), request.DedupKey, request.SourceMessageId, request.SourceModule, request.InitiatorUserId is { } initiator ? UserId.From(initiator) : null);
        notifications.Add(notification);
        var existing = await deliveries.FindExistingAsync(definition.Key, request.DedupKey, ct);
        var users = await contacts.GetAsync([.. request.To.Where(r => r.UserId is not null).Select(r => r.UserId!.Value).Distinct()], ct);
        var context = new FanOut(notification.Id, definition, request.DedupKey, content.Value, model.Value, request.HighPriority ? DeliveryPriority.High : DeliveryPriority.Normal, users, existing, time.GetUtcNow(), AttachmentsOf(request));
        foreach (var recipient in request.To)
        {
            Distribute(recipient, context);
        }

        return Result.Success();
    }

    // 内容和收件人无关，每个渠道渲染一次
    private async Task<Result<RenderedContent>> RenderAsync(NotificationTemplateDefinition definition, object model, CancellationToken ct)
    {
        var published = await templates.GetActiveVersionsAsync(definition.Key, ct);
        var channels = new Dictionary<TemplateChannel, ChannelContent>();
        foreach (var channel in TemplateChannels.From(definition.Channels))
        {
            var (subject, body, version) = published.TryGetValue(channel, out var active)
                ? (active.Subject, active.Body, active.Version)
                : (definition.DefaultSubject, TemplateChannels.DefaultBody(definition, channel), 0);
            var rendered = await ChannelRendering.RenderAsync(renderer, channel, subject, body, model, strict: false, ct);
            if (rendered.IsFailure)
            {
                return rendered.Error;
            }

            channels[channel] = rendered.Value with { Version = version };
        }

        return new RenderedContent(channels, TemplateHash.Of(definition));
    }

    // 本次各渠道实际用的版本号，0 表示代码里的默认内容
    private static string VersionsOf(RenderedContent content) =>
        JsonSerializer.Serialize(content.Channels.ToDictionary(pair => pair.Key.Code, pair => pair.Value.Version, StringComparer.Ordinal));

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
        var content = context.Content.For(TemplateChannel.InApp);
        var created = SiteMessage.Create(userId, context.NotificationId, content.Subject, content.Body, link, context.Definition.Module, context.Now);
        if (created.IsFailure)
        {
            created = SiteMessage.Create(userId, context.NotificationId, content.Subject, content.Body, null, context.Definition.Module, context.Now);
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
        var content = context.Content.For(channel == DeliveryChannel.Email ? TemplateChannel.Email : TemplateChannel.Sms);
        if (skip is not null || address is null)
        {
            deliveries.Add(Delivery.Skip(context.NotificationId, channel, userId, key, role, content.Subject, definition.Key, context.DedupKey, content.Version, context.Content.CodeHash, context.Priority, skip ?? DeliverySkipReason.NoAddress));
            return;
        }

        var masked = channel == DeliveryChannel.Email ? AddressMasking.Email(address) : AddressMasking.Phone(address);
        var direct = userId is null ? address : null;
        var parameters = channel == DeliveryChannel.Sms ? SmsParametersOf(context.Model) : null;
        var attachments = channel == DeliveryChannel.Email ? context.Attachments : null;
        if (capture.Value.Enabled)
        {
            var captureDelivery = Delivery.Capture(context.NotificationId, channel, userId, key, masked, direct, role, content.Subject, content.Body, parameters, attachments, definition.Key, context.DedupKey, content.Version, context.Content.CodeHash, context.Priority);
            deliveries.Add(captureDelivery);
            captured.Add(CapturedMessage.Create(captureDelivery.Id, channel, address, role, content.Subject, content.Body, context.Now));
            return;
        }

        deliveries.Add(Delivery.Queue(context.NotificationId, channel, userId, key, masked, direct, role, content.Subject, content.Body, parameters, attachments, definition.Key, context.DedupKey, content.Version, context.Content.CodeHash, context.Priority));
    }

    // 附件只有邮件用，存文件 ID，内容到发送时才读；重复的 ID 去掉，没有附件存空
    private static string? AttachmentsOf(NotificationRequest request) =>
        request.AttachmentFileIds is { Count: > 0 } ids ? JsonSerializer.Serialize(ids.Distinct()) : null;

    // 短信只能发已审核的模板编码加参数：参数随投递保存，发送时用
    private static string? SmsParametersOf(object model) =>
        model is ISmsTemplateModel sms ? JsonSerializer.Serialize(sms.ToSmsParameters()) : null;

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
        DateTimeOffset now,
        string? attachments)
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

        public string? Attachments => attachments;

        public HashSet<string> Seen { get; } = new(StringComparer.Ordinal);
    }
}
