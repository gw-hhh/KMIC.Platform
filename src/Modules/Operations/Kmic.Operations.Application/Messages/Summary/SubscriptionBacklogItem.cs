namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>单个订阅在收件箱中的积压情况。</summary>
/// <param name="Subscription">订阅名</param>
/// <param name="Pending">待处理和处理中的消息数</param>
/// <param name="OldestAgeSeconds">最早积压时长（秒），自消息接收起算</param>
/// <param name="Dead">死信条数</param>
/// <param name="Abandoned">已放弃消息数</param>
/// <param name="BlockedKeys">被死信阻塞的顺序键数</param>
/// <param name="IsRetiring">是否待终结</param>
public sealed record SubscriptionBacklogItem(string Subscription, long Pending, long OldestAgeSeconds, long Dead, long Abandoned, long BlockedKeys, bool IsRetiring);
