namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>模块 Outbox 中的待投递消息积压概况。</summary>
public sealed record OutboxBacklog(string Module, long Pending, TimeSpan OldestAge);
