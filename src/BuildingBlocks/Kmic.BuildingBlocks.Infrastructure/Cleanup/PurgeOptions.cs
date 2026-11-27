namespace Kmic.BuildingBlocks.Infrastructure.Cleanup;

/// <summary>框架清理任务配置</summary>
public sealed class PurgeOptions
{
    public const string Section = "Kmic:Purge";

    /// <summary>单批删除行数（每批独立提交，避免长时间锁表）</summary>
    public int BatchSize { get; set; } = 5000;

    /// <summary>补投窗口：已投递 Outbox 消息的保留时长，支持向新增或回滚后的订阅补投</summary>
    public TimeSpan RedeliveryWindow { get; set; } = TimeSpan.FromDays(7);

    /// <summary>定时任务执行记录保留时长</summary>
    public TimeSpan ScheduledRunRetention { get; set; } = TimeSpan.FromDays(30);

    /// <summary>执行记录处于运行中超过此时长后视为实例中断</summary>
    public TimeSpan AbandonRunningAfter { get; set; } = TimeSpan.FromHours(24);
}
