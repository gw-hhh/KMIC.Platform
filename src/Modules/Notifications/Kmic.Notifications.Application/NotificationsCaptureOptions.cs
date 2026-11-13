namespace Kmic.Notifications.Application;

/// <summary>
/// 是否把邮件和短信捕获下来而不真的发送
/// </summary>
/// <remarks>
/// 部署配置，节名 Notifications:Capture。默认捕获：开发、测试、预发布环境不能误发给真实的人，
/// 生产环境要明确配置成不捕获。捕获的内容在管理页面上查看
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
