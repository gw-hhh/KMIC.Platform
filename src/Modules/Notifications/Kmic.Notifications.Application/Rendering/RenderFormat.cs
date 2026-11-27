namespace Kmic.Notifications.Application.Rendering;

/// <summary>渲染输出格式</summary>
public enum RenderFormat
{
    /// <summary>邮件 HTML 正文（自动转义变量值）</summary>
    Html,

    /// <summary>主题、短信与站内消息纯文本（原样输出变量值）</summary>
    Text,
}
