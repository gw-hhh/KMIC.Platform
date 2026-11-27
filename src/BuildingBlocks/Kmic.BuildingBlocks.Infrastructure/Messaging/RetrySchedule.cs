namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal static class RetrySchedule
{
    // 计算第 attempts 次失败后的重试等待时长（attempts 从 1 开始）
    public static TimeSpan Delay(int attempts, MessagingOptions options)
    {
        var exponential = options.RetryBaseDelay.TotalSeconds * Math.Pow(options.RetryFactor, Math.Max(0, attempts - 1));
        var capped = Math.Min(exponential, options.RetryMaxDelay.TotalSeconds);

        // 附加随机抖动，避免批量失败的消息在同一时刻并发重试
        var jitter = 1 + (options.RetryJitter * ((Random.Shared.NextDouble() * 2) - 1));
        return TimeSpan.FromSeconds(capped * jitter);
    }
}
