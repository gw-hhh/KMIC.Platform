namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>小数精度配置（总位数与小数位数）</summary>
public sealed record DecimalPrecision(int Precision, int Scale);
