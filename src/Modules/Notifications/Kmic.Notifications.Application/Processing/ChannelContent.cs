namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 一个渠道渲染出的内容
/// </summary>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Version">实际用的发布版本号</param>
public sealed record ChannelContent(string Subject, string Body, int Version = 0);
