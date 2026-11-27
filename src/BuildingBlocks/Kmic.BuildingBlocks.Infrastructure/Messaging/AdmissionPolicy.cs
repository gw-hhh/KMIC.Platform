namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 容量保护状态判定策略
internal static class AdmissionPolicy
{
    internal const int LowWaterPeriodsToRecover = 2;

    // CapacityBytes 未配置时为 null，保持原状态不变
    internal sealed record Input(AdmissionState Previous, int LowWaterPeriods, long DatabaseBytes, long UnfinishedBytes, long? CapacityBytes);

    internal sealed record Decision(AdmissionState State, int LowWaterPeriods);

    internal static Decision Next(Input input, AdmissionOptions options)
    {
        if (input.CapacityBytes is not > 0)
        {
            return new Decision(input.Previous, input.LowWaterPeriods);
        }

        var capacity = (double)input.CapacityBytes.Value;
        if (input.DatabaseBytes >= capacity * options.HardRatio)
        {
            return new Decision(AdmissionState.Hard, 0);
        }

        var softConditionHolds = input.DatabaseBytes >= capacity * options.SoftRatio && input.UnfinishedBytes >= options.MinUnfinishedBytes;
        if (input.Previous == AdmissionState.Normal)
        {
            return new Decision(softConditionHolds ? AdmissionState.Soft : AdmissionState.Normal, 0);
        }

        // 原为软线或硬线刚解除时，需连续多个周期低于低水位方可恢复正常状态，不要求已用空间回落
        var periods = input.UnfinishedBytes <= options.LowWaterBytes ? input.LowWaterPeriods + 1 : 0;
        return periods >= LowWaterPeriodsToRecover
            ? new Decision(AdmissionState.Normal, 0)
            : new Decision(AdmissionState.Soft, periods);
    }
}
