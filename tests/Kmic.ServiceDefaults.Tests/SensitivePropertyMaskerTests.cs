using Serilog;
using Serilog.Events;
using Shouldly;

namespace Kmic.ServiceDefaults.Tests;

public sealed class SensitivePropertyMaskerTests
{
    private readonly CollectingSink _sink = new();

    [Theory]
    [InlineData("Password")]
    [InlineData("NewPassword")]
    [InlineData("accessToken")]
    [InlineData("ClientSecret")]
    [InlineData("Authorization")]
    [InlineData("ApiKey")]
    [InlineData("DbConnectionString")]
    public void SensitiveName_Masked(string name) => SensitivePropertyMasker.IsSensitive(name).ShouldBeTrue();

    [Theory]
    [InlineData("UserId")]
    [InlineData("OrderNumber")]
    [InlineData("Module")]
    public void OrdinaryName_NotMasked(string name) => SensitivePropertyMasker.IsSensitive(name).ShouldBeFalse();

    [Fact]
    public void TopLevelProperty_ValueReplaced_OthersUntouched()
    {
        Logger().Information("login {Password} by {UserName}", "p@ss", "alice");

        var logEvent = _sink.Events.Single();
        logEvent.Text("Password").ShouldBe("***");
        logEvent.Text("UserName").ShouldBe("alice");
    }

    [Fact]
    public void NestedObject_SensitiveMembersMasked()
    {
        Logger().Information("request {@Request}", new { UserName = "alice", Credentials = new { Token = "t", Hint = "h" }, ApiKey = "k" });

        var request = (StructureValue)_sink.Events.Single().Properties["Request"];
        LogEventExtensions.Text(request.Member("UserName")).ShouldBe("alice");
        LogEventExtensions.Text(request.Member("ApiKey")).ShouldBe("***");
        LogEventExtensions.Text(request.Member("Credentials")).ShouldBe("***");
    }

    [Fact]
    public void ObjectsInsideCollections_Masked()
    {
        Logger().Information("items {@Items}", new[] { new { Name = "a", Secret = "s1" }, new { Name = "b", Secret = "s2" } });

        var items = ((SequenceValue)_sink.Events.Single().Properties["Items"]).Elements.Cast<StructureValue>().ToList();
        items.Select(item => LogEventExtensions.Text(item.Member("Secret"))).ShouldAllBe(text => text == "***");
        items.Select(item => LogEventExtensions.Text(item.Member("Name"))).ShouldBe(["a", "b"]);
    }

    [Fact]
    public void DictionaryWithSensitiveKey_Masked()
    {
        Logger().Information("headers {@Headers}", new Dictionary<string, string>(StringComparer.Ordinal) { ["Authorization"] = "Bearer x", ["Accept"] = "json" });

        var headers = ((DictionaryValue)_sink.Events.Single().Properties["Headers"]).Elements.ToDictionary(e => LogEventExtensions.Text(e.Key), e => LogEventExtensions.Text(e.Value), StringComparer.Ordinal);
        headers["Authorization"].ShouldBe("***");
        headers["Accept"].ShouldBe("json");
    }

    [Fact]
    public void NothingSensitive_ValuesUnchanged()
    {
        Logger().Information("order {OrderNumber} {@Detail}", "SP-1", new { Count = 3 });

        var logEvent = _sink.Events.Single();
        logEvent.Text("OrderNumber").ShouldBe("SP-1");
        LogEventExtensions.Text(((StructureValue)logEvent.Properties["Detail"]).Member("Count")).ShouldBe("3");
    }

    private Serilog.Core.Logger Logger() => new LoggerConfiguration().Enrich.With<SensitivePropertyMasker>().WriteTo.Sink(_sink).CreateLogger();
}
