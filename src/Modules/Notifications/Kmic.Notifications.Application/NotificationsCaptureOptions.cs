namespace Kmic.Notifications.Application;

/// <summary>
/// 邮件和短信捕获配置，配置节 Notifications:Capture
/// </summary>
/// <remarks>
/// 默认开启，生产环境要关闭
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
