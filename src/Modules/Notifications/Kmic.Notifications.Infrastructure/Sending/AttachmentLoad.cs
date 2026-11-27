using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Sending;

internal sealed class AttachmentLoad
{
    private AttachmentLoad(IReadOnlyList<EmailAttachment> attachments, SendResult? failure)
    {
        Attachments = attachments;
        Failure = failure;
    }

    // 调用方负责释放内容流
    public IReadOnlyList<EmailAttachment> Attachments { get; }

    public SendResult? Failure { get; }

    public static AttachmentLoad Loaded(IReadOnlyList<EmailAttachment> attachments) => new(attachments, null);

    public static AttachmentLoad Failed(SendResult failure) => new([], failure);
}
