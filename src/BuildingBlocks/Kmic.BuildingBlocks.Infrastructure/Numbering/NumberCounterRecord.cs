namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>
/// 业务编号的计数
/// </summary>
/// <remarks>
/// 只用来建表，读写都在 NumberGenerator 里的一条 SQL 里完成。每个规则、周期、分组一行，不清理
/// </remarks>
internal sealed class NumberCounterRecord
{
    /// <summary>
    /// 规则名
    /// </summary>
    public string Rule { get; set; } = "";

    /// <summary>
    /// 周期键
    /// </summary>
    public string PeriodKey { get; set; } = "";

    /// <summary>
    /// 分组键，不分组时为 -
    /// </summary>
    public string PartitionKey { get; set; } = "";

    /// <summary>
    /// 最后发出的序号
    /// </summary>
    public long LastValue { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
