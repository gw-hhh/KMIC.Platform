namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>发件箱积压概况；<paramref name="OldestAge"/> 为最早滞留时长（自待投递消息进入起算）。</summary>
public sealed record OutboxBacklogInfo(long Pending, TimeSpan OldestAge, long Dead);
