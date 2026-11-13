namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>
/// 模板在一个渠道当前的内容
/// </summary>
/// <param name="Channel">渠道编码：Email、Sms、InApp</param>
/// <param name="ActiveVersion">生效的发布版本号，用代码里的默认内容时为空</param>
/// <param name="Subject">当前的主题；站内消息里是标题</param>
/// <param name="Body">当前的正文</param>
/// <param name="DefaultSubject">代码里的默认主题；代码里已经删掉的模板没有</param>
/// <param name="DefaultBody">代码里的默认正文；代码里已经删掉的模板没有</param>
public sealed record TemplateChannelInfo(string Channel, int? ActiveVersion, string Subject, string Body, string? DefaultSubject, string? DefaultBody);
