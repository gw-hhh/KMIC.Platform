namespace Kmic.Notifications.Application.Rendering;

/// <summary>
/// 渲染结果的用途
/// </summary>
public enum RenderFormat
{
    /// <summary>
    /// 邮件 HTML 正文：默认转义变量值，避免将输入内容解释为 HTML
    /// </summary>
    Html,

    /// <summary>
    /// 主题、短信、站内消息这类纯文本：变量的值原样输出
    /// </summary>
    Text,
}
