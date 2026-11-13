using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 模板的渠道：模板可以发往哪些渠道，每个渠道的内容单独发布版本
/// </summary>
public sealed class TemplateChannel : Enumeration<TemplateChannel>
{
    /// <summary>
    /// 邮件：主题加 HTML 正文
    /// </summary>
    public static readonly TemplateChannel Email = new("Email", "邮件");

    /// <summary>
    /// 短信：纯文本。实际发出的是阿里云审核过的模板加参数，这里的内容供查看
    /// </summary>
    public static readonly TemplateChannel Sms = new("Sms", "短信");

    /// <summary>
    /// 站内消息：标题加纯文本正文
    /// </summary>
    public static readonly TemplateChannel InApp = new("InApp", "站内消息");

    private TemplateChannel(string code, string name)
        : base(code, name)
    {
    }
}
