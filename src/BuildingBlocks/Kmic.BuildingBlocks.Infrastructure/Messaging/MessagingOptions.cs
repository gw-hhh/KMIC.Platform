namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 集成消息配置
/// </summary>
public sealed class MessagingOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Messaging";

    /// <summary>
    /// 单条消息内容的上限（字节）
    /// </summary>
    public int MaxPayloadBytes { get; set; } = 256 * 1024;

    /// <summary>
    /// 一个事务写入 Outbox 的上限（字节）
    /// </summary>
    public int MaxOutboxBytesPerTransaction { get; set; } = 1024 * 1024;

    /// <summary>
    /// 投递时每次领取的条数
    /// </summary>
    public int OutboxBatchSize { get; set; } = 100;

    /// <summary>
    /// 投递时领取后的占用时长
    /// </summary>
    public TimeSpan OutboxLease { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 处理时每次领取的条数
    /// </summary>
    public int InboxBatchSize { get; set; } = 20;

    /// <summary>
    /// 处理时领取后的占用时长
    /// </summary>
    /// <remarks>
    /// 处理器要在这段时间内完成，耗时的工作改成后台作业
    /// </remarks>
    public TimeSpan InboxLease { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 第一次重试前的等待时长
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 每次重试等待时长的倍数
    /// </summary>
    public double RetryFactor { get; set; } = 2;

    /// <summary>
    /// 重试等待时长的上限
    /// </summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 最多尝试次数，用完进入死信
    /// </summary>
    public int MaxAttempts { get; set; } = 10;

    /// <summary>
    /// 等待时长的随机浮动比例
    /// </summary>
    public double RetryJitter { get; set; } = 0.2;
}
