namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 量表定义
/// </summary>
/// <param name="Name">名称，如 kmic.notifications.pending</param>
/// <param name="Unit">单位，如 s、{delivery}</param>
/// <param name="Description">说明</param>
public sealed record BacklogGauge(string Name, string Unit, string Description);
