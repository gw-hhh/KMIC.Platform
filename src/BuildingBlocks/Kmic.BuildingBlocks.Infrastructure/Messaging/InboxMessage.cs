namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Inbox 中的一条消息
/// </summary>
/// <remarks>
/// 一行是一条消息对一个订阅。只用来建表，读写都是框架里的 SQL
/// </remarks>
internal sealed class InboxMessage
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 消息 ID
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// 订阅名
    /// </summary>
    public string Subscription { get; set; } = "";

    /// <summary>
    /// 消息名称
    /// </summary>
    public string MessageType { get; set; } = "";

    /// <summary>
    /// 消息版本
    /// </summary>
    public int SchemaVersion { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Payload { get; set; } = "";

    /// <summary>
    /// 信封中其余的字段
    /// </summary>
    public string Headers { get; set; } = "";

    /// <summary>
    /// 发布消息的模块
    /// </summary>
    public string SourceModule { get; set; } = "";

    /// <summary>
    /// 快照对应的 ID
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// 快照的来源版本
    /// </summary>
    public long? SourceVersion { get; set; }

    /// <summary>
    /// 顺序键
    /// </summary>
    public string? OrderingKey { get; set; }

    /// <summary>
    /// 发布顺序
    /// </summary>
    public long Seq { get; set; }

    /// <summary>
    /// 请求涉及的对象
    /// </summary>
    public Guid[]? SubjectIds { get; set; }

    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// 收到时间
    /// </summary>
    public DateTimeOffset ReceivedAt { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public short Status { get; set; }

    /// <summary>
    /// 处理次数
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// 下次处理时间
    /// </summary>
    public DateTimeOffset NextAttemptAt { get; set; }

    /// <summary>
    /// 占用令牌
    /// </summary>
    public Guid? LeaseToken { get; set; }

    /// <summary>
    /// 占用到期时间
    /// </summary>
    public DateTimeOffset? LeaseUntil { get; set; }

    /// <summary>
    /// 最近一次错误
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 处理完成时间
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
}
