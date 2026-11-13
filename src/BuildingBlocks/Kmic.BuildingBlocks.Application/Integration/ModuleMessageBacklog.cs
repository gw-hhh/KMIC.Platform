namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个模块的消息积压：发件箱一份，收件箱按订阅各一份
/// </summary>
/// <param name="Module">模块名</param>
/// <param name="Outbox">发件箱</param>
/// <param name="Subscriptions">收件箱里有积压、死信，或者本实例声明了的订阅</param>
public sealed record ModuleMessageBacklog(string Module, OutboxBacklogInfo Outbox, IReadOnlyList<SubscriptionBacklogInfo> Subscriptions);
