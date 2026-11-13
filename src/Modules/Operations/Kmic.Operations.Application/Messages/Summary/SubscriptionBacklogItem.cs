namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 一个订阅在收件箱里的积压
/// </summary>
/// <param name="Subscription">订阅名</param>
/// <param name="Pending">还没处理完的条数</param>
/// <param name="OldestAgeSeconds">最老的一条收到多少秒了；没有积压时为 0</param>
/// <param name="Dead">死信条数</param>
/// <param name="Abandoned">运维放弃的条数</param>
/// <param name="BlockedKeys">被死信挡住的顺序键个数，只有有序订阅才有</param>
/// <param name="IsRetiring">是不是待终结的订阅：积压是预期内的，不计入通用的积压告警</param>
public sealed record SubscriptionBacklogItem(string Subscription, long Pending, long OldestAgeSeconds, long Dead, long Abandoned, long BlockedKeys, bool IsRetiring);
