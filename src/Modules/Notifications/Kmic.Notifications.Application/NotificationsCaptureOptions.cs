namespace Kmic.Notifications.Application;

/// <summary>
/// 邮件和短信捕获配置
/// </summary>
/// <remarks>
/// 配置节为 Notifications:Capture，默认捕获邮件和短信，避免非生产环境误发。
/// 生产环境须显式关闭捕获；捕获内容可在管理页面查看
/// </remarks>
public sealed class NotificationsCaptureOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Capture";

    /// <summary>
    /// 捕获而不发送；默认开启，生产环境配置成 false
    /// </summary>
    public bool Enabled { get; init; } = true;
}
