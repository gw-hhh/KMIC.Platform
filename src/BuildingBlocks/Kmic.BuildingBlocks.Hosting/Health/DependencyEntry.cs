namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 一项依赖的状态
/// </summary>
/// <param name="Name">依赖名</param>
/// <param name="Status">Healthy、Degraded、Unhealthy</param>
/// <param name="Description">检查自己给出的说明；检查抛了异常时只写“检查失败”或“超时”，不带异常内容</param>
/// <param name="DurationMs">这一项的耗时</param>
internal sealed record DependencyEntry(string Name, string Status, string Description, double DurationMs);
