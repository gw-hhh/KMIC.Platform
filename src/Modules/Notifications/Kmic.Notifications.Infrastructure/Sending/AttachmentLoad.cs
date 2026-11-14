using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 附件读取结果
/// </summary>
internal sealed class AttachmentLoad
{
    private AttachmentLoad(IReadOnlyList<EmailAttachment> attachments, SendResult? failure)
    {
        Attachments = attachments;
        Failure = failure;
    }

    /// <summary>
    /// 已打开的附件，调用方须释放内容流
    /// </summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; }

    /// <summary>
    /// 读取失败时的发送结果，成功时为空
    /// </summary>
    public SendResult? Failure { get; }

    public static AttachmentLoad Loaded(IReadOnlyList<EmailAttachment> attachments) => new(attachments, null);

    public static AttachmentLoad Failed(SendResult failure) => new([], failure);
}
