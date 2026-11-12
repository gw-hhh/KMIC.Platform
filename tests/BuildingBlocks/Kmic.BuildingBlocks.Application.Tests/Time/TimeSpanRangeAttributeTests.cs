using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Application.Time;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Time;

public sealed class TimeSpanRangeAttributeTests
{
    private sealed class Options
    {
        [TimeSpanRange("00:01:00", "01:00:00")]
        public TimeSpan Lifetime { get; init; } = TimeSpan.FromMinutes(15);

        [TimeSpanRange("00:00:00", "90.00:00:00")]
        public TimeSpan? Optional { get; init; }
    }

    private static List<ValidationResult> Validate(Options options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }

    [Theory]
    [InlineData("00:01:00")]
    [InlineData("00:15:00")]
    [InlineData("01:00:00")]
    public void ValuesInsideTheRange_AreValid_IncludingBothEnds(string value) =>
        Validate(new Options { Lifetime = TimeSpan.Parse(value, System.Globalization.CultureInfo.InvariantCulture) }).ShouldBeEmpty();

    [Theory]
    [InlineData("00:00:59")]
    [InlineData("01:00:01")]
    public void ValuesOutsideTheRange_AreRejected_WithTheNameAndBounds(string value)
    {
        var results = Validate(new Options { Lifetime = TimeSpan.Parse(value, System.Globalization.CultureInfo.InvariantCulture) });

        var message = results.ShouldHaveSingleItem().ErrorMessage.ShouldNotBeNull();
        message.ShouldContain("Lifetime");
        message.ShouldContain("00:01:00");
        message.ShouldContain("01:00:00");
    }

    [Fact]
    public void AnUnsetOptionalValue_IsValid_AndDayBoundsAreParsed()
    {
        Validate(new Options { Optional = null }).ShouldBeEmpty();
        Validate(new Options { Optional = TimeSpan.FromDays(90) }).ShouldBeEmpty();
        Validate(new Options { Optional = TimeSpan.FromDays(91) }).ShouldHaveSingleItem();
    }

    [Fact]
    public void ABackwardsRange_IsRejectedWhenTheAttributeIsCreated() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new TimeSpanRangeAttribute("01:00:00", "00:01:00"));

    [Fact]
    public async Task ValidatingTheSameInstanceFromManyThreads_NeverThrows()
    {
        var tasks = Enumerable.Range(0, 64).Select(_ => Task.Run(
            () =>
            {
                for (var i = 0; i < 200; i++)
                {
                    Validate(new Options());
                }
            },
            TestContext.Current.CancellationToken));

        await Task.WhenAll(tasks);
    }
}
