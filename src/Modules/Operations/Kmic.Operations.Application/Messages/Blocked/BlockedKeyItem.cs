namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 一个被死信挡住的顺序键
/// </summary>
/// <param name="Module">订阅方模块</param>
/// <param name="Subscription">有序订阅的名称</param>
/// <param name="OrderingKey">顺序键，通常是聚合 ID</param>
/// <param name="DeadCount">这个键下的死信条数</param>
/// <param name="Waiting">排在最早的死信后面、还在等的条数</param>
/// <param name="FirstDeadId">最早的一条死信的行 ID：处理它才能放开这个键，重试与放弃都用它</param>
/// <param name="LastError">最早的一条死信最近一次的错误</param>
public sealed record BlockedKeyItem(string Module, string Subscription, string OrderingKey, long DeadCount, long Waiting, Guid FirstDeadId, string? LastError);
