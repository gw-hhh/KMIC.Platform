namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 发件箱的积压
/// </summary>
/// <param name="Pending">还没投递完的条数</param>
/// <param name="OldestAgeSeconds">最老的一条已经发布了多少秒；没有积压时为 0</param>
/// <param name="Dead">死信条数</param>
public sealed record OutboxBacklogItem(long Pending, long OldestAgeSeconds, long Dead);
