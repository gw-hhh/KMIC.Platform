using Microsoft.EntityFrameworkCore;
using Shouldly;
using Spike03.Domain;
using Xunit;

namespace Spike03.Tests;

public sealed class ComplexTypeTests(PostgresFixture db) : IClassFixture<PostgresFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Create_script_maps_each_shape_as_designed()
    {
        using var context = db.CreateContext();

        var script = context.Database.GenerateCreateScript();
        Log(script);

        script.ShouldContain("\"Email\" character varying(256) NOT NULL");
        script.ShouldContain("\"CreditLimitAmount\" numeric(18,2) NOT NULL");
        script.ShouldContain("\"CreditLimitCurrency\" character varying(3) NOT NULL");
        script.ShouldContain("\"ShippingAddressCity\" character varying(50),");
        script.ShouldContain("\"ShippingAddressPostalCode\" character varying(10),");
        script.ShouldContain("\"Contacts\" jsonb NOT NULL");
    }

    [Fact]
    public async Task All_shapes_round_trip()
    {
        var customer = Customer.Create("华东客户", EmailAddress.Create(" Buyer@Example.com "), Money.Create(1000m, "cny"));
        customer.ShipTo(Address.Create("杭州", "文三路 1 号", "310000"));
        customer.AddContact(ContactPerson.Create("张三", "13800000000", Money.Create(50m, "CNY")));
        customer.AddContact(ContactPerson.Create("李四"));
        await SaveAsync(customer);

        var loaded = await LoadAsync(customer.Id);

        loaded.Email.ShouldBe(EmailAddress.Create("buyer@example.com"));
        loaded.CreditLimit.ShouldBe(Money.Create(1000m, "CNY"));
        loaded.ShippingAddress.ShouldBe(Address.Create("杭州", "文三路 1 号", "310000"));
        loaded.Contacts.ShouldBe([ContactPerson.Create("张三", "13800000000", Money.Create(50m, "CNY")), ContactPerson.Create("李四")]);
    }

    [Fact]
    public async Task Optional_complex_type_switches_between_null_and_value()
    {
        var customer = NewCustomer();
        await SaveAsync(customer);
        (await LoadAsync(customer.Id)).ShippingAddress.ShouldBeNull();

        await UpdateAsync(customer.Id, c => c.ShipTo(Address.Create("上海", "南京路 2 号")));
        (await LoadAsync(customer.Id)).ShippingAddress.ShouldBe(Address.Create("上海", "南京路 2 号"));

        await UpdateAsync(customer.Id, c => c.ShipTo(null));
        (await LoadAsync(customer.Id)).ShippingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task Complex_collection_add_replace_remove_are_saved()
    {
        var customer = NewCustomer();
        customer.AddContact(ContactPerson.Create("甲"));
        customer.AddContact(ContactPerson.Create("乙"));
        await SaveAsync(customer);

        await UpdateAsync(customer.Id, c =>
        {
            c.ReplaceContact(0, ContactPerson.Create("甲", "13900000000"));
            c.RemoveContact(1);
            c.AddContact(ContactPerson.Create("丙", creditLimit: Money.Create(10m, "USD")));
        });

        (await LoadAsync(customer.Id)).Contacts.ShouldBe(
            [ContactPerson.Create("甲", "13900000000"), ContactPerson.Create("丙", creditLimit: Money.Create(10m, "USD"))]);
    }

    [Fact]
    public async Task Reading_skips_factory_validation()
    {
        var customer = NewCustomer();
        await SaveAsync(customer);
        await using (var context = db.CreateContext())
        {
            // 模拟规则收紧之前写入的旧数据：这两个值现在会被工厂方法拒绝
            await context.Database.ExecuteSqlAsync(
                $"UPDATE customer.\"Customers\" SET \"Email\" = 'legacy', \"CreditLimitAmount\" = -1 WHERE \"Id\" = {customer.Id}", Ct);
        }

        var loaded = await LoadAsync(customer.Id);

        loaded.Email.Value.ShouldBe("legacy");
        loaded.CreditLimit.Amount.ShouldBe(-1m);
    }

    [Fact]
    public async Task Queries_on_value_object_members_translate()
    {
        var marker = Guid.CreateVersion7().ToString("N")[^8..];
        var customer = Customer.Create(marker, EmailAddress.Create($"{marker}@example.com"), Money.Create(777m, "EUR"));
        customer.ShipTo(Address.Create(marker, "某路"));
        customer.AddContact(ContactPerson.Create(marker, creditLimit: Money.Create(5m, "EUR")));
        await SaveAsync(customer);
        await using var context = db.CreateContext();
        var email = EmailAddress.Create($"{marker}@example.com");

        IQueryable<Customer>[] queries =
        [
            context.Customers.Where(c => c.Email == email),
            context.Customers.Where(c => c.CreditLimit.Amount == 777m && c.CreditLimit.Currency == "EUR" && c.Name == marker),
            context.Customers.Where(c => c.ShippingAddress != null && c.ShippingAddress.City == marker),
            context.Customers.Where(c => EF.Property<List<ContactPerson>>(c, "_contacts").Any(p => p.Name == marker)),
            context.Customers.Where(c => EF.Property<List<ContactPerson>>(c, "_contacts").Any(p => p.CreditLimit != null && p.CreditLimit.Amount == 5m) && c.Name == marker),
        ];

        foreach (var query in queries)
        {
            Log(query.ToQueryString());
            (await query.Select(c => c.Id).ToListAsync(Ct)).ShouldBe([customer.Id]);
        }
    }

    [Fact]
    public async Task Change_tracker_gives_original_values_for_every_shape()
    {
        var customer = Customer.Create("审计", EmailAddress.Create("old@example.com"), Money.Create(100m, "CNY"));
        customer.AddContact(ContactPerson.Create("甲"));
        await SaveAsync(customer);

        await using var context = db.CreateContext();
        var loaded = await context.Customers.SingleAsync(c => c.Id == customer.Id, Ct);
        loaded.ChangeEmail(EmailAddress.Create("new@example.com"));
        loaded.ChangeCreditLimit(Money.Create(200m, "CNY"));
        loaded.ShipTo(Address.Create("杭州", "文三路 1 号"));
        loaded.ReplaceContact(0, ContactPerson.Create("甲", "13800000000"));
        context.ChangeTracker.DetectChanges();

        var changes = ChangeCollector.Collect(context.Entry(loaded));
        Log(string.Join('\n', changes));

        changes.ShouldBe(
        [
            "Email: old@example.com → new@example.com",
            "CreditLimit.Amount: 100.00 → 200",
            "ShippingAddress.City: null → 杭州",
            "ShippingAddress.Street: null → 文三路 1 号",
            """Contacts: [{"Name":"甲","Phone":null,"CreditLimit":null}] → [{"Name":"甲","Phone":"13800000000","CreditLimit":null}]""",
        ]);
    }

    [Fact]
    public async Task Change_tracker_reports_optional_complex_type_cleared()
    {
        var customer = NewCustomer();
        customer.ShipTo(Address.Create("杭州", "文三路 1 号"));
        await SaveAsync(customer);

        await using var context = db.CreateContext();
        var loaded = await context.Customers.SingleAsync(c => c.Id == customer.Id, Ct);
        loaded.ShipTo(null);
        context.ChangeTracker.DetectChanges();

        var changes = ChangeCollector.Collect(context.Entry(loaded));
        Log(string.Join('\n', changes));

        changes.ShouldBe(["ShippingAddress.City: 杭州 → null", "ShippingAddress.Street: 文三路 1 号 → null"]);
    }

    private static Customer NewCustomer() =>
        Customer.Create("客户", EmailAddress.Create("c@example.com"), Money.Create(1m, "CNY"));

    private async Task SaveAsync(Customer customer)
    {
        await using var context = db.CreateContext();
        context.Customers.Add(customer);
        await context.SaveChangesAsync(Ct);
    }

    private async Task UpdateAsync(Guid id, Action<Customer> change)
    {
        await using var context = db.CreateContext();
        var customer = await context.Customers.SingleAsync(c => c.Id == id, Ct);
        change(customer);
        await context.SaveChangesAsync(Ct);
    }

    private async Task<Customer> LoadAsync(Guid id)
    {
        await using var context = db.CreateContext();
        return await context.Customers.AsNoTracking().SingleAsync(c => c.Id == id, Ct);
    }

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text + "\n");
}
