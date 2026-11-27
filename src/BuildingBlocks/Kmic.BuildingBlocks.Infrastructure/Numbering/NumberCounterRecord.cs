namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

internal sealed class NumberCounterRecord
{
    public string Rule { get; set; } = "";

    public string PeriodKey { get; set; } = "";

    // 不分组时为 "-"
    public string PartitionKey { get; set; } = "";

    public long LastValue { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
