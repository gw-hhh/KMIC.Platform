using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 容量保护的状态判断，容量取 1000 字节，软线 0.8、硬线 0.9，软线要求队列不少于 100、低水位 20
/// </summary>
public sealed class AdmissionPolicyTests
{
    private static readonly AdmissionOptions _options = new() { SoftRatio = 0.8, HardRatio = 0.9, MinUnfinishedBytes = 100, LowWaterBytes = 20 };

    [Theory]
    [InlineData(799, 500)]
    [InlineData(850, 99)]
    public void Normal_StaysNormalUnlessSpaceAndQueueAreBothOver(long database, long unfinished)
    {
        var decision = Next(AdmissionState.Normal, 0, database, unfinished);

        decision.State.ShouldBe(AdmissionState.Normal);
    }

    [Fact]
    public void SpaceOverSoftLineAndQueueLarge_BecomesSoft()
    {
        Next(AdmissionState.Normal, 0, database: 800, unfinished: 100).State.ShouldBe(AdmissionState.Soft);
    }

    [Theory]
    [InlineData(AdmissionState.Normal)]
    [InlineData(AdmissionState.Soft)]
    [InlineData(AdmissionState.Hard)]
    public void SpaceOverHardLine_AlwaysHard_EvenWhenQueueIsEmpty(AdmissionState previous)
    {
        var decision = Next(previous, 1, database: 900, unfinished: 0);

        decision.State.ShouldBe(AdmissionState.Hard);
        decision.LowWaterPeriods.ShouldBe(0);
    }

    [Fact]
    public void Soft_NeedsTwoConsecutiveLowPeriodsToRecover()
    {
        var first = Next(AdmissionState.Soft, 0, database: 850, unfinished: 20);
        first.State.ShouldBe(AdmissionState.Soft);
        first.LowWaterPeriods.ShouldBe(1);

        var second = Next(AdmissionState.Soft, first.LowWaterPeriods, database: 850, unfinished: 20);
        second.State.ShouldBe(AdmissionState.Normal);
    }

    [Fact]
    public void Soft_QueueRisingAgain_ResetsTheCount()
    {
        var decision = Next(AdmissionState.Soft, 1, database: 850, unfinished: 21);

        decision.State.ShouldBe(AdmissionState.Soft);
        decision.LowWaterPeriods.ShouldBe(0);
    }

    [Fact]
    public void Soft_DoesNotRequireSpaceToFall_OnlyTheQueue()
    {
        // 表膨胀让已用空间一直高于软线时，队列排空就能恢复
        var decision = Next(AdmissionState.Soft, 1, database: 890, unfinished: 0);

        decision.State.ShouldBe(AdmissionState.Normal);
    }

    [Fact]
    public void HardJustCleared_FallsToSoft_AndNeedsTwoLowPeriodsBeforeNormal()
    {
        var first = Next(AdmissionState.Hard, 0, database: 700, unfinished: 0);
        first.State.ShouldBe(AdmissionState.Soft);
        first.LowWaterPeriods.ShouldBe(1);

        Next(AdmissionState.Hard, 1, database: 700, unfinished: 0).State.ShouldBe(AdmissionState.Normal);
    }

    [Theory]
    [InlineData(AdmissionState.Normal)]
    [InlineData(AdmissionState.Soft)]
    [InlineData(AdmissionState.Hard)]
    public void CapacityNotSet_KeepsThePreviousState(AdmissionState previous)
    {
        var decision = AdmissionPolicy.Next(new AdmissionPolicy.Input(previous, 1, 999_999, 999_999, null), _options);

        decision.State.ShouldBe(previous);
        decision.LowWaterPeriods.ShouldBe(1);
    }

    private static AdmissionPolicy.Decision Next(AdmissionState previous, int lowWaterPeriods, long database, long unfinished) =>
        AdmissionPolicy.Next(new AdmissionPolicy.Input(previous, lowWaterPeriods, database, unfinished, 1000), _options);
}
