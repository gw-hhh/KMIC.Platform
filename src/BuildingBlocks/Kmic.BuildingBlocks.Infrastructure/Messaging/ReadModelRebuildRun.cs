namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 读模型的一轮重建
/// </summary>
/// <remarks>
/// 只用来建表，读写都是框架里的 SQL。扫描位置在作业的检查点里，这里不重复存
/// </remarks>
internal sealed class ReadModelRebuildRun
{
    /// <summary>
    /// 轮次 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 订阅名
    /// </summary>
    public string Subscription { get; set; } = "";

    /// <summary>
    /// 读模型类型名
    /// </summary>
    public string ReadModel { get; set; } = "";

    /// <summary>
    /// 执行这一轮的后台作业
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public short Status { get; set; }

    /// <summary>
    /// 已扫描的行数
    /// </summary>
    public long ScannedCount { get; set; }

    /// <summary>
    /// 源端确认不存在的行数，暂停等确认时是已发现的数量
    /// </summary>
    public long MissingCount { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }
}
