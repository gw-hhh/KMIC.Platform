namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 邮件的一个附件
/// </summary>
/// <param name="Name">附件的文件名</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Content">内容的流，由调用方释放</param>
public sealed record EmailAttachment(string Name, string ContentType, Stream Content);
