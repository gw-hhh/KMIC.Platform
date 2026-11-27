namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>常用小数精度预设（占位默认值，业务规则确认前勿强依赖具体位数）</summary>
public static class Precisions
{
    public static readonly DecimalPrecision Amount = new(18, 2);

    public static readonly DecimalPrecision UnitPrice = new(18, 4);

    public static readonly DecimalPrecision Quantity = new(18, 4);

    /// <summary>比率（如税率、折扣率）</summary>
    public static readonly DecimalPrecision Rate = new(9, 6);
}
