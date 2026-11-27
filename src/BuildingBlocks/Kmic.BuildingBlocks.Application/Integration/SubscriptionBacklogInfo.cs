namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>单个订阅的收件箱积压概况；<paramref name="OldestAge"/> 自未处理消息接收起算。</summary>
public sealed record SubscriptionBacklogInfo(string Subscription, long Pending, TimeSpan OldestAge, long Dead, long Abandoned, long BlockedKeys);
