using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>模板支持发送的渠道（各渠道独立发布版本）</summary>
public sealed class TemplateChannel : Enumeration<TemplateChannel>
{
    /// <summary>邮件（主题 + HTML 正文）</summary>
    public static readonly TemplateChannel Email = new("Email", "邮件");

    /// <summary>短信（纯文本仅用于预览，实际发送使用已审核的模板编码与参数）</summary>
    public static readonly TemplateChannel Sms = new("Sms", "短信");

    /// <summary>站内消息（标题 + 纯文本正文）</summary>
    public static readonly TemplateChannel InApp = new("InApp", "站内消息");

    private TemplateChannel(string code, string name)
        : base(code, name)
    {
    }
}
