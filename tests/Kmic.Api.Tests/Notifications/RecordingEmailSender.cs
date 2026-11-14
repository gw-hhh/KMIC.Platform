using System.Collections.Concurrent;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 记录发了什么的邮件渠道，测试里换掉真实的 SMTP；返回什么由测试决定
/// </summary>
/// <remarks>
/// 附件的流发完就被释放，所以发送的时候就把内容读出来存着
/// </remarks>
internal sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<ReceivedAttachment>> _attachments = new();
    private int _calls;

    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Func<EmailMessage, int, SendResult> Behavior { get; set; } = (_, _) => SendResult.Sent("fake-email");

    /// <summary>
    /// 某封邮件发送时带的附件；没有发过或没有附件时为空
    /// </summary>
    public IReadOnlyList<ReceivedAttachment> AttachmentsOf(Guid deliveryId) =>
        _attachments.GetValueOrDefault(deliveryId) ?? [];

    public async Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        var received = new List<ReceivedAttachment>();
        foreach (var attachment in message.Attachments)
        {
            using var buffer = new MemoryStream();
            await attachment.Content.CopyToAsync(buffer, ct);
            received.Add(new ReceivedAttachment(attachment.Name, attachment.ContentType, buffer.ToArray()));
        }

        _attachments[message.DeliveryId] = received;
        Sent.Enqueue(message);
        return Behavior(message, Interlocked.Increment(ref _calls));
    }

    /// <summary>
    /// 收到的一个附件
    /// </summary>
    internal sealed record ReceivedAttachment(string Name, string ContentType, byte[] Content);
}
