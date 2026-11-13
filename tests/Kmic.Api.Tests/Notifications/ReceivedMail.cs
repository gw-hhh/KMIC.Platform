namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 假 SMTP 服务器收到的一封邮件
/// </summary>
/// <param name="From">信封里的发件人</param>
/// <param name="Recipients">信封里的收件人，包含密送</param>
/// <param name="Data">DATA 的全部内容，包括邮件头</param>
public sealed record ReceivedMail(string From, IReadOnlyList<string> Recipients, string Data);
