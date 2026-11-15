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
/// 处理投递队列，在数据库事务之外调用发送渠道，再按领取令牌记录结果
/// </summary>
/// <remarks>
/// 队列循环由框架提供。发送时查询系统内用户的最新联系方式；用户不存在、已禁用或无地址时记为已跳过。
/// 含密投递超过创建后的有效期时不再发送；成功、最终失败或跳过时清空正文及短信参数。
/// 暂时失败退避重试，达到次数上限或永久失败时标记为失败，等待人工处理
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
            await RecordAsync(store => store.FailedAsync(item, token, "含密通知已过期，未发送", clearSecret: true, ct), ct);
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

    // 系统内用户使用 Identity 中的最新地址；外部收件人使用投递记录中的地址
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "未取消时，将发送异常记为暂时失败并退避重试，避免单条投递中断队列处理")]
    private async Task<SendResult> SendAsync(DeliveryItem item, string senderProfile, string address, CancellationToken ct)
    {
        try
        {
            if (string.Equals(item.Channel, DeliveryChannel.Email.Code, StringComparison.Ordinal))
            {
                return await SendEmailAsync(item, senderProfile, address, ct);
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

    // 有附件的邮件发送时才读文件内容，流在发完之后释放；读不到附件按暂时或永久失败记在投递上
    private async Task<SendResult> SendEmailAsync(DeliveryItem item, string senderProfile, string address, CancellationToken ct)
    {
        var ids = item.AttachmentFileIds is { } json ? JsonSerializer.Deserialize<List<Guid>>(json) ?? [] : [];
        if (ids.Count == 0)
        {
            return await email.SendAsync(new EmailMessage(item.Id, senderProfile, address, item.Role, item.Subject, item.Body ?? string.Empty, []), ct);
        }

        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        var load = await ActivatorUtilities.CreateInstance<AttachmentLoader>(scope.ServiceProvider)
            .LoadAsync(ids, options.Value.MaxAttachmentMb * 1024L * 1024L, ct);
        if (load.Failure is { } failure)
        {
            return failure;
        }

        try
        {
            return await email.SendAsync(new EmailMessage(item.Id, senderProfile, address, item.Role, item.Subject, item.Body ?? string.Empty, load.Attachments), ct);
        }
        finally
        {
            foreach (var attachment in load.Attachments)
            {
                await attachment.Content.DisposeAsync();
            }
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
