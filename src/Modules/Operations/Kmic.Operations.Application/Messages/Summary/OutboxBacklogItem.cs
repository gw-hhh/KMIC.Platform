namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>发件箱积压。</summary>
/// <param name="Pending">待投递和投递中的消息数</param>
/// <param name="OldestAgeSeconds">最早积压时长（秒），自消息发生起算</param>
/// <param name="Dead">死信条数</param>
public sealed record OutboxBacklogItem(long Pending, long OldestAgeSeconds, long Dead);
