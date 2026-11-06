using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spike03.Domain;

namespace Spike03.Infrastructure;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(256);

        builder.ComplexProperty(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.ComplexProperty(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.City).HasMaxLength(50);
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.PostalCode).HasMaxLength(10);
        });

        // 公开属性是只读集合，EF 10 的复杂集合要求 IList<T>，所以映射私有字段，列名用公开属性名
        builder.Ignore(c => c.Contacts);
        builder.ComplexCollection<List<ContactPerson>, ContactPerson>("_contacts", contacts => contacts.ToJson("Contacts"));
    }
}
