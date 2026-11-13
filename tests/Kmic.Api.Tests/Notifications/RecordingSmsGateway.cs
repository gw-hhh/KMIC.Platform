using System.Collections.Concurrent;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 记录发了什么的短信渠道
/// </summary>
internal sealed class RecordingSmsGateway : ISmsGateway
{
    private int _calls;

    public ConcurrentQueue<SmsMessage> Sent { get; } = new();

    public Func<SmsMessage, int, SendResult> Behavior { get; set; } = (_, _) => SendResult.Sent("fake-sms");

    public Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct)
    {
        Sent.Enqueue(message);
        return Task.FromResult(Behavior(message, Interlocked.Increment(ref _calls)));
    }
}
