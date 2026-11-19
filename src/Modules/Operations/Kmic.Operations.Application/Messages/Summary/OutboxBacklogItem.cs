namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 发件箱的积压
/// </summary>
/// <param name="Pending">待投递和投递中的消息数</param>
/// <param name="OldestAgeSeconds">最早积压消息距发生时间的秒数</param>
/// <param name="Dead">死信条数</param>
public sealed record OutboxBacklogItem(long Pending, long OldestAgeSeconds, long Dead);
