namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>集成消息配置</summary>
public sealed class MessagingOptions
{
    public const string Section = "Kmic:Messaging";

    /// <summary>单条消息载荷上限（字节）</summary>
    public int MaxPayloadBytes { get; set; } = 256 * 1024;

    /// <summary>单个事务写入 Outbox 的总字节上限</summary>
    public int MaxOutboxBytesPerTransaction { get; set; } = 1024 * 1024;

    /// <summary>Outbox 单次领取条数</summary>
    public int OutboxBatchSize { get; set; } = 100;

    /// <summary>Outbox 领取租约时长</summary>
    public TimeSpan OutboxLease { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Inbox 单次领取条数</summary>
    public int InboxBatchSize { get; set; } = 20;

    /// <summary>Inbox 领取租约时长（处理器须在此时间内完成，耗时操作请拆分为后台作业）</summary>
    public TimeSpan InboxLease { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>首次重试等待时长</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>退避重试间隔乘数</summary>
    public double RetryFactor { get; set; } = 2;

    /// <summary>重试最大等待时长</summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>最大重试次数，耗尽后转入死信队列</summary>
    public int MaxAttempts { get; set; } = 10;

    /// <summary>重试间隔随机抖动比例</summary>
    public double RetryJitter { get; set; } = 0.2;

    /// <summary>兜底轮询间隔（仅在 PG NOTIFY 丢失时补充唤醒）</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>实例心跳上报间隔</summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>单个模块 Inbox 消息的并发处理上限</summary>
    public int InboxParallelism { get; set; } = 4;

    /// <summary>停机等待在途消息完成的宽限时长，超时后强制取消</summary>
    public TimeSpan ShutdownGrace { get; set; } = TimeSpan.FromSeconds(30);

    public AdmissionOptions Admission { get; } = new();

    public RebuildOptions Rebuild { get; } = new();
}
