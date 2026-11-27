using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Channels;

internal sealed class UnconfiguredSmsGateway : ISmsGateway
{
    public Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct) =>
        Task.FromResult(SendResult.Permanent("短信网关未配置，未发送"));
}
