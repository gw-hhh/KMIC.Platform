namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 一次发送请求渲染出的内容：与收件人无关，各渠道各取所需
/// </summary>
/// <param name="Subject">主题，纯文本；站内消息里是标题</param>
/// <param name="HtmlBody">邮件正文，变量的值已转义</param>
/// <param name="TextBody">短信与站内消息的正文，纯文本</param>
/// <param name="CodeHash">默认内容的哈希，投递记录里留一份，事后能知道发出的是哪一版</param>
public sealed record RenderedContent(string Subject, string HtmlBody, string TextBody, string CodeHash);
