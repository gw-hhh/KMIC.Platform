using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Shouldly;

namespace Kmic.Identity.Domain.Tests.Users;

public sealed class MobileNumbersTests
{
    [Theory]
    [InlineData("13800138000", "+8613800138000")]
    [InlineData("138 0013 8000", "+8613800138000")]
    [InlineData("138-0013-8000", "+8613800138000")]
    [InlineData("8613800138000", "+8613800138000")]
    [InlineData("+86 138 0013 8000", "+8613800138000")]
    [InlineData("008613800138000", "+8613800138000")]
    [InlineData("+14155552671", "+14155552671")]
    [InlineData("+44 20 7946 0958", "+442079460958")]
    public void Normalize_AcceptedForms_AreWrittenInInternationalFormat(string input, string expected) =>
        MobileNumbers.Normalize(input).ShouldBeSuccess().ShouldBe(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("23800138000")]
    [InlineData("1380013800")]
    [InlineData("138001380001")]
    [InlineData("+8612345678901")]
    [InlineData("abcdefghijk")]
    [InlineData("+0123456789")]
    public void Normalize_Rejected(string? input) =>
        MobileNumbers.Normalize(input).ShouldBeFailure(IdentityErrors.MobileInvalid);
}
