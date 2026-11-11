namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 常用小数精度
/// </summary>
/// <remarks>
/// 目前是占位值，具体位数等业务确认。确认前不要在业务表里依赖这里的小数位
/// </remarks>
public static class Precisions
{
    /// <summary>
    /// 金额
    /// </summary>
    public static readonly DecimalPrecision Amount = new(18, 2);

    /// <summary>
    /// 单价
    /// </summary>
    public static readonly DecimalPrecision UnitPrice = new(18, 4);

    /// <summary>
    /// 数量
    /// </summary>
    public static readonly DecimalPrecision Quantity = new(18, 4);

    /// <summary>
    /// 比率，如税率、折扣率
    /// </summary>
    public static readonly DecimalPrecision Rate = new(9, 6);
}
