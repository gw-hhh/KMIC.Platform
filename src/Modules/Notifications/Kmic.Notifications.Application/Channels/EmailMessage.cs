namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 待发送邮件
/// </summary>
/// <param name="DeliveryId">投递 ID</param>
/// <param name="SenderProfile">发件账号名</param>
/// <param name="Address">收件地址</param>
/// <param name="Role">To、Cc 或 Bcc</param>
/// <param name="Subject">主题</param>
/// <param name="HtmlBody">HTML 正文</param>
/// <param name="Attachments">邮件附件</param>
public sealed record EmailMessage(Guid DeliveryId, string SenderProfile, string Address, string Role, string Subject, string HtmlBody, IReadOnlyList<EmailAttachment> Attachments);
