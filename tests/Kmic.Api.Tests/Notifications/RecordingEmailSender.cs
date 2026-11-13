using System.Collections.Concurrent;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 记录发了什么的邮件渠道，测试里换掉真实的 SMTP；返回什么由测试决定
/// </summary>
internal sealed class RecordingEmailSender : IEmailSender
{
    private int _calls;

    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Func<EmailMessage, int, SendResult> Behavior { get; set; } = (_, _) => SendResult.Sent("fake-email");

    public Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        Sent.Enqueue(message);
        return Task.FromResult(Behavior(message, Interlocked.Increment(ref _calls)));
    }
}
