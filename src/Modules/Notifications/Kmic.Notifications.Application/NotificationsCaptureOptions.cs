namespace Kmic.Notifications.Application;

/// <summary>邮件与短信捕获配置（默认开启，生产环境须关闭）</summary>
public sealed class NotificationsCaptureOptions
{
    public const string Section = "Notifications:Capture";

    /// <summary>是否启用捕获模式（默认开启，生产环境须设为 false）</summary>
    public bool Enabled { get; init; } = true;
}
