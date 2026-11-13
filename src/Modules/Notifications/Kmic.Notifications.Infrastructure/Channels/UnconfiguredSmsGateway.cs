using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>
/// 还没有接真实短信服务时的占位实现：每条都明确地永久失败
/// </summary>
/// <remarks>
/// 生产环境要用阿里云短信，等开通并对照官方文档实现后替换。在那之前，非生产环境靠捕获查看内容，
/// 生产环境发短信会留下一条说明原因的失败记录，不会悄悄丢掉
/// </remarks>
internal sealed class UnconfiguredSmsGateway : ISmsGateway
{
    public Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct) =>
        Task.FromResult(SendResult.Permanent("短信渠道还没有配置真实的网关，没有发送"));
}
