namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 依赖状态的响应内容
/// </summary>
/// <param name="Status">整体状态：Healthy、Degraded、Unhealthy</param>
/// <param name="TotalDurationMs">全部检查并行执行的总耗时</param>
/// <param name="Entries">每项依赖的状态，按名称排序</param>
internal sealed record DependencyReport(string Status, double TotalDurationMs, IReadOnlyList<DependencyEntry> Entries);
