namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 容量保护的判断：由上一轮的状态和这一轮的采样得出新状态
/// </summary>
internal static class AdmissionPolicy
{
    // 解除软线要连续这么多个周期的未完成消息量不高于低水位
    internal const int LowWaterPeriodsToRecover = 2;

    /// <param name="Previous">上一轮的状态</param>
    /// <param name="LowWaterPeriods">上一轮已连续低于低水位的周期数</param>
    /// <param name="DatabaseBytes">数据库已用空间</param>
    /// <param name="UnfinishedBytes">各模块 Outbox、Inbox 里未完成的消息量</param>
    /// <param name="CapacityBytes">已分配容量；还没有设置时为空，这时保持原状态</param>
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

        // 原来是软线，或者硬线刚解除：队列要连续几个周期降到低水位才恢复，不要求已用空间回落
        var periods = input.UnfinishedBytes <= options.LowWaterBytes ? input.LowWaterPeriods + 1 : 0;
        return periods >= LowWaterPeriodsToRecover
            ? new Decision(AdmissionState.Normal, 0)
            : new Decision(AdmissionState.Soft, periods);
    }
}
