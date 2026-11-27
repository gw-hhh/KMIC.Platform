namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>模板渠道内容</summary>
/// <param name="Channel">渠道编码：Email、Sms、InApp</param>
/// <param name="ActiveVersion">当前选用的发布版本号，未选用时为空</param>
/// <param name="Subject">当前主题</param>
/// <param name="Body">当前正文</param>
/// <param name="DefaultSubject">代码默认主题，声明已移除时为空</param>
/// <param name="DefaultBody">代码默认正文，声明已移除时为空</param>
public sealed record TemplateChannelInfo(string Channel, int? ActiveVersion, string Subject, string Body, string? DefaultSubject, string? DefaultBody);
