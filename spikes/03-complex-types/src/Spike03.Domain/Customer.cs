namespace Spike03.Domain;

/// <summary>
/// 示例聚合：覆盖 05-03 第 3.4 节的四种值对象形态
/// </summary>
public sealed class Customer
{
    private readonly List<ContactPerson> _contacts = [];

    private Customer(Guid id, string name, EmailAddress email, Money creditLimit)
    {
        Id = id;
        Name = name;
        Email = email;
        CreditLimit = creditLimit;
    }

    // EF 使用
    private Customer()
    {
        Name = null!;
        Email = null!;
        CreditLimit = null!;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public EmailAddress Email { get; private set; }

    public Money CreditLimit { get; private set; }

    public Address? ShippingAddress { get; private set; }

    public IReadOnlyList<ContactPerson> Contacts => _contacts;

    public static Customer Create(string name, EmailAddress email, Money creditLimit) =>
        new(Guid.CreateVersion7(), name, email, creditLimit);

    public void ChangeCreditLimit(Money creditLimit) => CreditLimit = creditLimit;

    public void ChangeEmail(EmailAddress email) => Email = email;

    public void ShipTo(Address? address) => ShippingAddress = address;

    public void AddContact(ContactPerson contact) => _contacts.Add(contact);

    public void ReplaceContact(int index, ContactPerson contact) => _contacts[index] = contact;

    public void RemoveContact(int index) => _contacts.RemoveAt(index);
}
