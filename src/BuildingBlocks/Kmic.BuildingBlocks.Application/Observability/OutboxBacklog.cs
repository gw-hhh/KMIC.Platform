namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 一个模块 Outbox 里还没投递完的消息
/// </summary>
/// <param name="Module">模块名</param>
/// <param name="Pending">条数</param>
/// <param name="OldestAge">最老一条产生至今</param>
public sealed record OutboxBacklog(string Module, long Pending, TimeSpan OldestAge);
