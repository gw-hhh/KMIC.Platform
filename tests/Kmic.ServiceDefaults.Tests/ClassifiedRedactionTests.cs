using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Shouldly;

namespace Kmic.ServiceDefaults.Tests;

public sealed class ClassifiedRedactionTests
{
    private readonly CollectingSink _sink = new();

    [Theory]
    [InlineData("13812345678", "138****5678")]
    [InlineData("1234567", "*******")]
    [InlineData("zhang@example.com", "z***@example.com")]
    public void Personal_PartiallyMasked(string value, string expected) =>
        Provider().GetRedactor(KmicData.Personal).Redact(value).ShouldBe(expected);

    [Fact]
    public void Secret_ReplacedWithoutRevealingTheLength()
    {
        var redactor = Provider().GetRedactor(KmicData.Secret);

        redactor.Redact("a").ShouldBe("***");
        redactor.Redact("a-very-long-secret-value").ShouldBe("***");
    }

    [Fact]
    public void Internal_PassedThrough() => Provider().GetRedactor(KmicData.Internal).Redact("SP-202611-0001").ShouldBe("SP-202611-0001");

    [Fact]
    public void DestructuredObject_ClassifiedPropertiesAreRedacted_OthersKept()
    {
        Logger().Information("customer {@Customer}", new Customer("SP-1", "13812345678", "zhang@example.com", "pw123456", Notes: null));

        var customer = (StructureValue)_sink.Events.Single().Properties["Customer"];
        LogEventExtensions.Text(customer.Member("Number")).ShouldBe("SP-1");
        LogEventExtensions.Text(customer.Member("Phone")).ShouldBe("138****5678");
        LogEventExtensions.Text(customer.Member("Email")).ShouldBe("z***@example.com");
        LogEventExtensions.Text(customer.Member("PasswordHash")).ShouldBe("***");
        customer.Properties.Select(p => p.Name).ShouldBe(["Number", "Phone", "Email", "PasswordHash", "Notes"]);
    }

    [Fact]
    public void NullClassifiedValue_StaysNull()
    {
        Logger().Information("customer {@Customer}", new Customer("SP-1", null, null, null, null));

        var customer = (StructureValue)_sink.Events.Single().Properties["Customer"];
        ((ScalarValue)customer.Member("Phone")).Value.ShouldBeNull();
    }

    [Fact]
    public void ObjectInsideACollection_Redacted()
    {
        Logger().Information("customers {@Customers}", new[] { new Customer("A", "13800000001", null, null, null) });

        var items = ((SequenceValue)_sink.Events.Single().Properties["Customers"]).Elements.Cast<StructureValue>().ToList();
        LogEventExtensions.Text(items.Single().Member("Phone")).ShouldBe("138****0001");
    }

    [Fact]
    public void TypeWithoutAnyClassification_UsesTheDefaultDestructuring()
    {
        Logger().Information("plain {@Plain}", new Plain("x", 3));

        var plain = (StructureValue)_sink.Events.Single().Properties["Plain"];
        LogEventExtensions.Text(plain.Member("Name")).ShouldBe("x");
        plain.TypeTag.ShouldBe("Plain");
    }

    private static IRedactorProvider Provider()
    {
        var services = new ServiceCollection();
        services.AddKmicRedaction();
        return services.BuildServiceProvider().GetRequiredService<IRedactorProvider>();
    }

    private Serilog.Core.Logger Logger() => new LoggerConfiguration().Destructure.With<ClassifiedDestructuringPolicy>().WriteTo.Sink(_sink).CreateLogger();

    private sealed record Customer(
        string Number,
        [property: PersonalData] string? Phone,
        [property: PersonalData] string? Email,
        [property: SecretData] string? PasswordHash,
        string? Notes);

    private sealed record Plain(string Name, int Count);
}
