namespace Spike03.Domain;

/// <summary>
/// 多值值对象：复杂类型，每个成员一列
/// </summary>
public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // EF 物化使用，跳过校验（05-01 第 5.3 节）
    private Money() => Currency = null!;

    public decimal Amount { get; private init; }

    public string Currency { get; private init; }

    public static Money Create(decimal amount, string currency) =>
        amount < 0 ? throw new ArgumentOutOfRangeException(nameof(amount)) : new Money(amount, currency.ToUpperInvariant());
}
