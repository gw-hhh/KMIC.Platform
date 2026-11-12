using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Tests.Serialization;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Querying;

public sealed class EnumerationFilterTests
{
    [Fact]
    public void Codes_BecomeTheMatchingValues_InTheOrderGiven()
    {
        var result = EnumerationFilter.Parse<Shade>(["Blue", "Red"], "status");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe([Shade.Blue, Shade.Red]);
    }

    [Fact]
    public void NoCodes_MeansNoFilter()
    {
        EnumerationFilter.Parse<Shade>(null, "status").Value.ShouldBeEmpty();
        EnumerationFilter.Parse<Shade>([], "status").Value.ShouldBeEmpty();
    }

    [Fact]
    public void AnUnknownCode_IsAFieldError_NotAnEmptyMatch()
    {
        var result = EnumerationFilter.Parse<Shade>(["Red", "Green"], "status");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.ValidationFailed");
        var error = result.Error.FieldErrors.ShouldHaveSingleItem();
        error.Path.ShouldBe("status");
        error.Message.ShouldContain("Green");
    }
}
