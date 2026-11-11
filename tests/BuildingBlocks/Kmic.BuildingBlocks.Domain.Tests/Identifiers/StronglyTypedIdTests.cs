using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Tests.Fakes;
using Shouldly;
using Vogen;

namespace Kmic.BuildingBlocks.Domain.Tests.Identifiers;

/// <summary>
/// 强类型 ID 的 Vogen 配置
/// </summary>
public sealed class StronglyTypedIdTests
{
    [Fact]
    public void New_GeneratesVersion7()
    {
        var id = OrderId.New();

        id.Value.Version.ShouldBe(7);
    }

    [Fact]
    public void From_WhenEmpty_Rejected()
    {
        Should.Throw<ValueObjectValidationException>(() => OrderId.From(Guid.Empty));
        OrderId.TryFrom(Guid.Empty, out _).ShouldBeFalse();
    }

    [Fact]
    public void Json_RoundTripsAsString()
    {
        var id = OrderId.New();

        var json = JsonSerializer.Serialize(id);

        json.ShouldBe($"\"{id.Value}\"");
        JsonSerializer.Deserialize<OrderId>(json).ShouldBe(id);
    }

    [Fact]
    public void TryParse_WhenNotGuid_ReturnsFalse()
    {
        OrderId.TryParse("not-a-guid", null, out _).ShouldBeFalse();
        OrderId.TryParse(Guid.Empty.ToString(), null, out _).ShouldBeFalse();
    }
}
