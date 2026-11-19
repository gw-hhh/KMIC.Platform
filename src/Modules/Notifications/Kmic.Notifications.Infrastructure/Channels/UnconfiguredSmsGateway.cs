using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>
/// 短信网关占位实现，所有发送请求均返回永久失败
/// </summary>
internal sealed class UnconfiguredSmsGateway : ISmsGateway
{
    public Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct) =>
        Task.FromResult(SendResult.Permanent("短信网关未配置，未发送"));
}
