namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 多值值对象：每个成员一列，列名为“属性名 + 成员名”
/// </summary>
internal sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; private init; }

    /// <summary>
    /// 币种，ISO 4217 三位代码
    /// </summary>
    public string Currency { get; private init; }

    public static Money Of(decimal amount, string currency) => new(amount, currency);
}
