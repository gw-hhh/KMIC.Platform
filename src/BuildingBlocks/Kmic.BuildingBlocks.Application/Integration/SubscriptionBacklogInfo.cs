namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个订阅在收件箱里的积压
/// </summary>
/// <param name="Subscription">订阅名</param>
/// <param name="Pending">还没处理完的条数：待处理加处理中</param>
/// <param name="OldestAge">最老的一条收到多久了；没有积压时为零</param>
/// <param name="Dead">死信条数：处理一直失败或确定不能处理</param>
/// <param name="Abandoned">运维放弃的条数</param>
/// <param name="BlockedKeys">被死信挡住的顺序键个数；只有有序订阅才有，无序订阅里的死信不挡别的消息</param>
public sealed record SubscriptionBacklogInfo(string Subscription, long Pending, TimeSpan OldestAge, long Dead, long Abandoned, long BlockedKeys);
