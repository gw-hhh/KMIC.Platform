namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 一类后台作业的排队情况
/// </summary>
/// <param name="Module">模块名</param>
/// <param name="Type">作业类型名</param>
/// <param name="Queued">排队条数</param>
/// <param name="OldestQueued">最老一条排队至今</param>
public sealed record JobBacklog(string Module, string Type, long Queued, TimeSpan OldestQueued);
