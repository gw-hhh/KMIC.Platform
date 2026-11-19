namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>
/// 业务编号的计数
/// </summary>
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
