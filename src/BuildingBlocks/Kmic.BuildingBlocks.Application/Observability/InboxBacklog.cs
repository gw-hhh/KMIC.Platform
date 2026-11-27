namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>订阅在 Inbox 中的积压概况。</summary>
/// <param name="OldestAge">最早一条未处理消息接收至今时长。</param>
/// <param name="BlockedKeys">有序订阅中被死信阻塞的顺序键数量。</param>
/// <param name="Retiring">订阅是否处于待终结状态。</param>
public sealed record InboxBacklog(string Module, string Subscription, long Pending, TimeSpan OldestAge, long Dead, long BlockedKeys, bool Retiring);
