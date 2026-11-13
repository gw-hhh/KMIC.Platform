namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 一个渠道渲染出的内容
/// </summary>
/// <param name="Subject">主题，纯文本；站内消息里是标题</param>
/// <param name="Body">正文：邮件是 HTML，变量的值已转义；短信和站内消息是纯文本</param>
/// <param name="Version">实际用的发布版本号，0 表示代码里的默认内容</param>
public sealed record ChannelContent(string Subject, string Body, int Version = 0);
