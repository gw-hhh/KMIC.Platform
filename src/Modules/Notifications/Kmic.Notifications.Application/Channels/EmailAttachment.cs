namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 邮件附件，内容流由调用方释放
/// </summary>
public sealed record EmailAttachment(string Name, string ContentType, Stream Content);
