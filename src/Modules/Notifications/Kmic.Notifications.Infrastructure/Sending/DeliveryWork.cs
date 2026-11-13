using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.Notifications.Application.Channels;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Deliveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 投递队列：领取待发送的投递，在事务之外调用渠道，再用带令牌条件的语句记录结果
/// </summary>
/// <remarks>
/// 领取循环由框架提供，这里只写怎样领取、怎样发一条。发送时向 Identity 取收件人最新的联系方式，
/// 已经禁用或没有地址的记为已跳过。含密模板有有效期，过了不再发送，发完或最终失败就清空正文。
/// 暂时失败的按尝试次数退避，次数用完或永久失败的转为失败，等人工处理
/// </remarks>
internal sealed class DeliveryWork(
    IExecutionScopeFactory scopes,
    NotificationTemplateCatalog catalog,
    IEmailSender email,
    ISmsGateway sms,
    IOptions<NotificationsDeliveryOptions> options,
    TimeProvider time,
    ILogger<DeliveryWork> logger)
    : ILeasedWork<DeliveryItem>
{
    private static readonly TimeSpan _secretLifetime = TimeSpan.FromSeconds(SendSecretNotification.LifetimeSeconds);

    public string Name => "Notifications.Delivery";

    public int Parallelism => options.Value.Parallelism;

    public TimeSpan PollInterval => options.Value.Poll;

    public async Task<IReadOnlyList<DeliveryItem>> ClaimAsync(int max, Guid token, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        return await ActivatorUtilities.CreateInstance<DeliveryStore>(scope.ServiceProvider).ClaimAsync(max, token, options.Value.Lease, ct);
    }

    public async Task ProcessAsync(DeliveryItem item, Guid token, CancellationToken ct)
    {
        var definition = catalog.Find(item.TemplateKey);
        if (definition is null)
        {
            await RecordAsync(store => store.FailedAsync(item, token, "模板已经不存在", clearSecret: true, ct), ct);
            return;
        }

        var secret = definition.CarriesSecret;
        if (secret && time.GetUtcNow() - item.CreatedAt > _secretLifetime)
        {
            await RecordAsync(store => store.FailedAsync(item, token, "含密通知已经过了有效期，没有发送", clearSecret: true, ct), ct);
            return;
        }

        var target = await ResolveAddressAsync(item, ct);
        if (target.Skip is not null)
        {
            await RecordAsync(store => store.SkippedAsync(item, token, target.Skip, secret, ct), ct);
            return;
        }

        var result = await SendAsync(item, definition.SenderProfile, target.Address!, ct);
        await RecordResultAsync(item, token, result, secret, ct);
    }

    // 用户收件人向 Identity 取最新的联系方式；直接给的地址就是记录里的那个
    private async Task<(string? Address, DeliverySkipReason? Skip)> ResolveAddressAsync(DeliveryItem item, CancellationToken ct)
    {
        if (item.RecipientUserId is not { } userId)
        {
            return string.IsNullOrWhiteSpace(item.Address) ? (null, DeliverySkipReason.NoAddress) : (item.Address, null);
        }

        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        var contact = await ActivatorUtilities.CreateInstance<ContactLookup>(scope.ServiceProvider).FindAsync(userId, ct);
        if (contact is null)
        {
            return (null, DeliverySkipReason.UnknownUser);
        }

        if (!contact.IsActive)
        {
            return (null, DeliverySkipReason.UserDisabled);
        }

        var address = string.Equals(item.Channel, DeliveryChannel.Email.Code, StringComparison.Ordinal) ? contact.Email : contact.Mobile;
        return string.IsNullOrWhiteSpace(address) ? (null, DeliverySkipReason.NoAddress) : (address.Trim(), null);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "渠道客户端的任何异常（网络、超时、协议）都按暂时失败处理：记录原因、退避重试，不能让这一条影响别的")]
    private async Task<SendResult> SendAsync(DeliveryItem item, string senderProfile, string address, CancellationToken ct)
    {
        try
        {
            if (string.Equals(item.Channel, DeliveryChannel.Email.Code, StringComparison.Ordinal))
            {
                return await email.SendAsync(new EmailMessage(item.Id, senderProfile, address, item.Role, item.Subject, item.Body ?? string.Empty), ct);
            }

            var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(item.SmsParameters ?? "{}") ?? [];
            return await sms.SendAsync(new SmsMessage(item.Id, item.TemplateKey, address, parameters), ct);
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            NotificationsLog.ChannelCallFailed(logger, exception, item.Id, item.Channel);
            return SendResult.Transient($"调用渠道时出错：{exception.GetType().Name}");
        }
    }

    private async Task RecordResultAsync(DeliveryItem item, Guid token, SendResult result, bool secret, CancellationToken ct)
    {
        switch (result.Outcome)
        {
            case SendOutcome.Sent:
                await RecordAsync(store => store.SentAsync(item, token, result.ProviderMessageId, secret, ct), ct);
                break;
            case SendOutcome.Transient when item.Attempts < options.Value.MaxAttempts:
                await RecordAsync(store => store.RetryLaterAsync(item, token, result.Error ?? "暂时失败", ct), ct);
                break;
            default:
                await RecordAsync(store => store.FailedAsync(item, token, result.Error ?? "发送失败", secret, ct), ct);
                break;
        }
    }

    private async Task RecordAsync(Func<DeliveryStore, Task> write, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        ct.ThrowIfCancellationRequested();
        await write(ActivatorUtilities.CreateInstance<DeliveryStore>(scope.ServiceProvider));
    }
}
