namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>被死信阻塞的顺序键；<paramref name="Waiting"/> 为阻塞在最早死信后的待处理消息数，<paramref name="LastError"/> 取自最早死信。</summary>
public sealed record BlockedKey(string Module, string Subscription, string OrderingKey, long DeadCount, long Waiting, Guid FirstDeadId, string? LastError);
