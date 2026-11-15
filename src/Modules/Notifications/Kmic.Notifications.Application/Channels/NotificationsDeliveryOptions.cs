using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 投递工作器的部署配置
/// </summary>
public sealed class NotificationsDeliveryOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Delivery";

    /// <summary>
    /// 并行投递数
    /// </summary>
    [Range(1, 32)]
    public int Parallelism { get; init; } = 4;

    /// <summary>
    /// 暂时失败时，领取次数小于此值才会自动重试
    /// </summary>
    [Range(1, 20)]
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// 领取租期（分钟），到期后其他实例可接管
    /// </summary>
    [Range(1, 60)]
    public int LeaseMinutes { get; init; } = 5;

    /// <summary>
    /// 无可领取投递时的轮询间隔（秒）
    /// </summary>
    [Range(1, 300)]
    public int PollSeconds { get; init; } = 5;

    /// <summary>
    /// 单封邮件的附件总大小上限（MB），超过时判为永久失败
    /// </summary>
    [Range(1, 100)]
    public int MaxAttachmentMb { get; init; } = 20;

    public TimeSpan Lease => TimeSpan.FromMinutes(LeaseMinutes);

    public TimeSpan Poll => TimeSpan.FromSeconds(PollSeconds);
}
