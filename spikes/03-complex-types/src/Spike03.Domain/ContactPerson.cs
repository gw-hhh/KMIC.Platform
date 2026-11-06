namespace Spike03.Domain;

/// <summary>
/// 值对象集合的元素：复杂类型集合，整体存为一个 jsonb 列
/// </summary>
public sealed record ContactPerson
{
    private ContactPerson(string name, string? phone, Money? creditLimit)
    {
        Name = name;
        Phone = phone;
        CreditLimit = creditLimit;
    }

    private ContactPerson() => Name = null!;

    public string Name { get; private init; }

    public string? Phone { get; private init; }

    // 集合元素中再嵌套一个可选值对象
    public Money? CreditLimit { get; private init; }

    public static ContactPerson Create(string name, string? phone = null, Money? creditLimit = null) => new(name, phone, creditLimit);
}
