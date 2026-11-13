namespace Kmic.Notifications.Application.Rendering;

/// <summary>
/// 渲染结果的用途
/// </summary>
public enum RenderFormat
{
    /// <summary>
    /// 邮件的 HTML 正文：变量的值默认转义，防止用户输入的内容变成标记
    /// </summary>
    Html,

    /// <summary>
    /// 主题、短信、站内消息这类纯文本：变量的值原样输出
    /// </summary>
    Text,
}
