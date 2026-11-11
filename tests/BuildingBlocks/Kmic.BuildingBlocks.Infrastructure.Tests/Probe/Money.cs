namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 金额
/// </summary>
internal sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// 数额
    /// </summary>
    public decimal Amount { get; private init; }

    /// <summary>
    /// 币种
    /// </summary>
    public string Currency { get; private init; }

    public static Money Of(decimal amount, string currency) => new(amount, currency);
}
