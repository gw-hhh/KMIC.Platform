namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 导出作业的结果摘要
/// </summary>
/// <param name="Rows">导出的行数</param>
internal sealed record ExportSummary(long Rows);
