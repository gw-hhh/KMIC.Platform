namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>模块消息积压概况（包含发件箱及各收件箱订阅）。</summary>
public sealed record ModuleMessageBacklog(string Module, OutboxBacklogInfo Outbox, IReadOnlyList<SubscriptionBacklogInfo> Subscriptions);
