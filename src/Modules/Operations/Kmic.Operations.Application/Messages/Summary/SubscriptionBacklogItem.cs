namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 一个订阅在收件箱里的积压
/// </summary>
/// <param name="Subscription">订阅名</param>
/// <param name="Pending">待处理和处理中的消息数，不含死信</param>
/// <param name="OldestAgeSeconds">最早积压消息距接收时间的秒数，无积压时为 0</param>
/// <param name="Dead">死信条数</param>
/// <param name="Abandoned">运维放弃的条数</param>
/// <param name="BlockedKeys">被死信挡住的顺序键个数，只有有序订阅才有</param>
/// <param name="IsRetiring">是否待终结；此类积压应单独告警</param>
public sealed record SubscriptionBacklogItem(string Subscription, long Pending, long OldestAgeSeconds, long Dead, long Abandoned, long BlockedKeys, bool IsRetiring);
