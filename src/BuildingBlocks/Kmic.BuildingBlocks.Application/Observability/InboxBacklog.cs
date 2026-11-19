namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 一个订阅在 Inbox 里的积压
/// </summary>
/// <param name="Module">订阅方模块</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Pending">待处理和处理中的条数</param>
/// <param name="OldestAge">最老一条收到至今</param>
/// <param name="Dead">死信条数</param>
/// <param name="BlockedKeys">有序订阅里被死信挡住的顺序键个数</param>
/// <param name="Retiring">订阅处于待终结状态</param>
public sealed record InboxBacklog(string Module, string Subscription, long Pending, TimeSpan OldestAge, long Dead, long BlockedKeys, bool Retiring);
