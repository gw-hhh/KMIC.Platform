using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 投递工作器的部署配置
/// </summary>
/// <remarks>
/// 节名 Notifications:Delivery
/// </remarks>
public sealed class NotificationsDeliveryOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Delivery";

    /// <summary>
    /// 同时发送几条
    /// </summary>
    [Range(1, 32)]
    public int Parallelism { get; init; } = 4;

    /// <summary>
    /// 一条最多尝试几次：用完还没发出去的转为失败，等人工处理
    /// </summary>
    [Range(1, 20)]
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// 领取之后多少分钟没有结果，别的实例可以接管
    /// </summary>
    [Range(1, 60)]
    public int LeaseMinutes { get; init; } = 5;

    /// <summary>
    /// 没有可领取的投递时，隔多少秒查一次队列
    /// </summary>
    [Range(1, 300)]
    public int PollSeconds { get; init; } = 5;

    /// <summary>
    /// 一封邮件的附件加起来最多多少 MB，超过的这封邮件不发（永久失败）
    /// </summary>
    [Range(1, 100)]
    public int MaxAttachmentMb { get; init; } = 20;

    /// <summary>
    /// 领取的租期
    /// </summary>
    public TimeSpan Lease => TimeSpan.FromMinutes(LeaseMinutes);

    /// <summary>
    /// 轮询间隔
    /// </summary>
    public TimeSpan Poll => TimeSpan.FromSeconds(PollSeconds);
}
