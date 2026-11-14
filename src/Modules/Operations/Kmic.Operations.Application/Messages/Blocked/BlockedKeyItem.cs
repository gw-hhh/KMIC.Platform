namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 被死信阻塞的顺序键
/// </summary>
/// <param name="Module">订阅方模块</param>
/// <param name="Subscription">有序订阅的名称</param>
/// <param name="OrderingKey">顺序键，通常是聚合 ID</param>
/// <param name="DeadCount">这个键下的死信条数</param>
/// <param name="Waiting">等待最早死信处理完成的消息数</param>
/// <param name="FirstDeadId">最早死信的记录 ID，用于重试或放弃；处理后可推进该顺序键</param>
/// <param name="LastError">最早的一条死信最近一次的错误</param>
public sealed record BlockedKeyItem(string Module, string Subscription, string OrderingKey, long DeadCount, long Waiting, Guid FirstDeadId, string? LastError);
