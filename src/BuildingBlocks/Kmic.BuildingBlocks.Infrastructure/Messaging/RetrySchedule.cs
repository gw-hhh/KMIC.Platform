namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 失败后下一次重试的等待时长
/// </summary>
internal static class RetrySchedule
{
    /// <summary>
    /// 第 attempts 次失败后的等待时长，attempts 从 1 开始
    /// </summary>
    public static TimeSpan Delay(int attempts, MessagingOptions options)
    {
        var exponential = options.RetryBaseDelay.TotalSeconds * Math.Pow(options.RetryFactor, Math.Max(0, attempts - 1));
        var capped = Math.Min(exponential, options.RetryMaxDelay.TotalSeconds);

        // 加随机浮动，同时失败的一批消息不会在同一时刻一起重试
        var jitter = 1 + (options.RetryJitter * ((Random.Shared.NextDouble() * 2) - 1));
        return TimeSpan.FromSeconds(capped * jitter);
    }
}
