namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed class ReadModelRebuildRun
{
    public Guid Id { get; set; }

    public string Subscription { get; set; } = "";

    public string ReadModel { get; set; } = "";

    public Guid JobId { get; set; }

    public short Status { get; set; }

    public long ScannedCount { get; set; }

    // 源端确认不存在的行数（暂停等确认时为已发现数）
    public long MissingCount { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }
}
