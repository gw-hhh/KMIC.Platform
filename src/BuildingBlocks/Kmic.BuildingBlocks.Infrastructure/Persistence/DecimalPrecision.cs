namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 小数精度
/// </summary>
/// <param name="Precision">总位数</param>
/// <param name="Scale">小数位数</param>
public sealed record DecimalPrecision(int Precision, int Scale);
