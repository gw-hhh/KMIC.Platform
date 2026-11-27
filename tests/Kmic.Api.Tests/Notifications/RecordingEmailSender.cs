using System.Collections.Concurrent;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Api.Tests.Notifications;

// 替代真实 SMTP 发送；附件流发送后即释放，发送时即时缓存附件字节
internal sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<ReceivedAttachment>> _attachments = new();
    private int _calls;

    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Func<EmailMessage, int, SendResult> Behavior { get; set; } = (_, _) => SendResult.Sent("fake-email");

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

    internal sealed record ReceivedAttachment(string Name, string ContentType, byte[] Content);
}
