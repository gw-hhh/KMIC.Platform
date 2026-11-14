namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 要发出去的一封邮件
/// </summary>
/// <param name="DeliveryId">投递 ID，邮件头 Message-ID 由它生成，稳定不变，帮助收件端去重</param>
/// <param name="SenderProfile">发件账号名，对应部署配置里的一项</param>
/// <param name="Address">收件地址</param>
/// <param name="Role">To、Cc 或 Bcc</param>
/// <param name="Subject">主题</param>
/// <param name="HtmlBody">HTML 正文</param>
/// <param name="Attachments">附件；内容的流由发送方读，发完之后调用方负责释放</param>
public sealed record EmailMessage(Guid DeliveryId, string SenderProfile, string Address, string Role, string Subject, string HtmlBody, IReadOnlyList<EmailAttachment> Attachments);
