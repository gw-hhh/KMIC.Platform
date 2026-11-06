namespace Spike03.Domain;

/// <summary>
/// 可选的多值值对象：EF 10 可选复杂类型
/// </summary>
public sealed record Address
{
    private Address(string city, string street, string? postalCode)
    {
        City = city;
        Street = street;
        PostalCode = postalCode;
    }

    private Address()
    {
        City = null!;
        Street = null!;
    }

    public string City { get; private init; }

    public string Street { get; private init; }

    public string? PostalCode { get; private init; }

    public static Address Create(string city, string street, string? postalCode = null) => new(city, street, postalCode);
}
