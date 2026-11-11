namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 框架清理任务的配置
/// </summary>
/// <remarks>
/// 各保留期的具体天数还没和业务确认，先用这里的默认值
/// </remarks>
public sealed class PurgeOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Purge";

    /// <summary>
    /// 每批删除的行数，批与批之间各自提交，不长时间锁表
    /// </summary>
    public int BatchSize { get; set; } = 5000;

    /// <summary>
    /// 补投窗口：已投递的 Outbox 消息保留多久，期间可以补投给新订阅或回滚后的订阅
    /// </summary>
    public TimeSpan RedeliveryWindow { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// 定时任务执行记录保留多久
    /// </summary>
    public TimeSpan ScheduledRunRetention { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// 执行记录停在“执行中”超过这么久，就当执行它的进程已经没了
    /// </summary>
    public TimeSpan AbandonRunningAfter { get; set; } = TimeSpan.FromHours(24);
}
