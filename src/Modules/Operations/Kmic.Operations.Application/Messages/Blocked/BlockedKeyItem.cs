namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>被死信阻塞的顺序键。</summary>
/// <param name="Module">订阅方模块</param>
/// <param name="Subscription">有序订阅的名称</param>
/// <param name="OrderingKey">顺序键</param>
/// <param name="DeadCount">该顺序键下的死信数</param>
/// <param name="Waiting">阻塞中的消息数，等待最早死信处理完成</param>
/// <param name="FirstDeadId">最早死信的记录 ID</param>
/// <param name="LastError">最早死信的最近错误</param>
public sealed record BlockedKeyItem(string Module, string Subscription, string OrderingKey, long DeadCount, long Waiting, Guid FirstDeadId, string? LastError);
