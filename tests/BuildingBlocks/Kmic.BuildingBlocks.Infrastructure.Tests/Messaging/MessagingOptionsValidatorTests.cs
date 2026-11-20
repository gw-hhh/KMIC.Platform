using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 容量保护配置的启动校验：互相矛盾的数值拒绝启动，并说明是哪一项
/// </summary>
public sealed class MessagingOptionsValidatorTests
{
    private readonly MessagingOptionsValidator _validator = new();

    [Fact]
    public void TheDefaults_AreValid() => _validator.Validate(null, new MessagingOptions()).Succeeded.ShouldBeTrue();

    [Theory]
    [InlineData(0.95, 0.85)]
    [InlineData(0.9, 0.9)]
    [InlineData(0.0, 0.9)]
    [InlineData(0.8, 1.5)]
    [InlineData(1.0, 1.0)]
    public void SoftAndHardRatios_MustBeOrderedWithinZeroAndOne(double soft, double hard)
    {
        var options = new MessagingOptions { Admission = { SoftRatio = soft, HardRatio = hard } };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldHaveSingleItem().ShouldContain("SoftRatio");
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(100, 200)]
    [InlineData(100, -1)]
    public void LowWater_MustBeBelowTheSoftLineQueueSize(long min, long lowWater)
    {
        var options = new MessagingOptions { Admission = { MinUnfinishedBytes = min, LowWaterBytes = lowWater } };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldHaveSingleItem().ShouldContain("LowWaterBytes");
    }

    [Fact]
    public void ASampleIntervalUnderOneSecond_IsRefused()
    {
        var options = new MessagingOptions { Admission = { SampleInterval = TimeSpan.FromMilliseconds(100) } };

        _validator.Validate(null, options).Failures.ShouldHaveSingleItem().ShouldContain("SampleInterval");
    }

    [Fact]
    public void SeveralProblems_AreAllReported()
    {
        var options = new MessagingOptions { Admission = { SampleInterval = TimeSpan.Zero, SoftRatio = 0.95, HardRatio = 0.85, MinUnfinishedBytes = 0 } };

        _validator.Validate(null, options).Failures!.Count().ShouldBeGreaterThanOrEqualTo(3);
    }
}
