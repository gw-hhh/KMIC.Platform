namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 常用数值的精度，用法 .HasPrecision(Precisions.Amount)
/// </summary>
/// <remarks>
/// 具体位数待业务确认（ADR-0012 第 4.1 条），目前是占位值；确认前不要在业务表中依赖这里的小数位
/// </remarks>
public static class Precisions
{
    /// <summary>
    /// 金额，单位元
    /// </summary>
    public static readonly DecimalPrecision Amount = new(18, 2);

    /// <summary>
    /// 单价，单位元
    /// </summary>
    public static readonly DecimalPrecision UnitPrice = new(18, 4);

    /// <summary>
    /// 数量
    /// </summary>
    public static readonly DecimalPrecision Quantity = new(18, 4);

    /// <summary>
    /// 比率（税率、折扣率等），0.13 表示 13%
    /// </summary>
    public static readonly DecimalPrecision Rate = new(9, 6);
}
