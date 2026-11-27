namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>单模块消息积压。</summary>
/// <param name="Module">模块名</param>
/// <param name="Outbox">发件箱</param>
/// <param name="Subscriptions">收件箱各订阅积压情况</param>
public sealed record ModuleBacklogItem(string Module, OutboxBacklogItem Outbox, IReadOnlyList<SubscriptionBacklogItem> Subscriptions);
