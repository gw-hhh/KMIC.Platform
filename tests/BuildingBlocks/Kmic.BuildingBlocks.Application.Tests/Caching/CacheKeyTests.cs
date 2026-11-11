using System.Globalization;
using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Caching;

public sealed class CacheKeyTests
{
    [Fact]
    public void Of_JoinsSegmentsWithInvariantFormatting()
    {
        var roleId = RoleId.From(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"));
        var previous = CultureInfo.CurrentCulture;
        try
        {
            // 小数点是逗号的区域设置下，键也不能变
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            CacheKey.Of("Identity", "RoleSnapshot", roleId, 42L).ShouldBe("Identity:RoleSnapshot:0f8fad5b-d9cb-469f-a165-70867728950e:42");
            CacheKey.Of("Sample", "Rate", 1.5m).ShouldBe("Sample:Rate:1.5");
            CacheKey.Of("Configuration", "Dictionary").ShouldBe("Configuration:Dictionary");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("", "Thing")]
    [InlineData("Sample", "Thing:Item")]
    [InlineData("Sample", " ")]
    public void Of_BadSegment_Throws(string module, string item) =>
        Should.Throw<ArgumentException>(() => CacheKey.Of(module, item));

    [Fact]
    public void Of_IdentifierWithSeparator_Throws() =>
        Should.Throw<ArgumentException>(() => CacheKey.Of("Sample", "Thing", "a:b"));
}
