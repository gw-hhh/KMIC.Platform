using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application.Channels;

/// <summary>投递后台任务部署配置</summary>
public sealed class NotificationsDeliveryOptions
{
    public const string Section = "Notifications:Delivery";

    /// <summary>并行投递数</summary>
    [Range(1, 32)]
    public int Parallelism { get; init; } = 4;

    /// <summary>自动重试上限（暂时失败且领取次数小于此值时自动重试）</summary>
    [Range(1, 20)]
    public int MaxAttempts { get; init; } = 5;

    /// <summary>领取占用时长（分钟），到期后可由其他实例接管</summary>
    [Range(1, 60)]
    public int LeaseMinutes { get; init; } = 5;

    /// <summary>无待投递记录时的轮询间隔（秒）</summary>
    [Range(1, 300)]
    public int PollSeconds { get; init; } = 5;

    /// <summary>单封邮件附件总大小上限（MB），超出判为永久失败</summary>
    [Range(1, 100)]
    public int MaxAttachmentMb { get; init; } = 20;

    public TimeSpan Lease => TimeSpan.FromMinutes(LeaseMinutes);

    public TimeSpan Poll => TimeSpan.FromSeconds(PollSeconds);
}
