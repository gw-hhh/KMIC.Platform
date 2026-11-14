using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 读取附件的结果：成功时是打开的附件，失败时是应该记到投递上的发送结果
/// </summary>
internal sealed class AttachmentLoad
{
    private AttachmentLoad(IReadOnlyList<EmailAttachment> attachments, SendResult? failure)
    {
        Attachments = attachments;
        Failure = failure;
    }

    /// <summary>
    /// 打开的附件，调用方发完之后释放它们的流
    /// </summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; }

    /// <summary>
    /// 失败的结果；成功时为空
    /// </summary>
    public SendResult? Failure { get; }

    public static AttachmentLoad Loaded(IReadOnlyList<EmailAttachment> attachments) => new(attachments, null);

    public static AttachmentLoad Failed(SendResult failure) => new([], failure);
}
