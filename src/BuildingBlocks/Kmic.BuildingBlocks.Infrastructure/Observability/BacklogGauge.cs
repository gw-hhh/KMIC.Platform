namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 量表定义
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Unit">单位</param>
/// <param name="Description">说明</param>
public sealed record BacklogGauge(string Name, string Unit, string Description);
