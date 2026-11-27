namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>投递表影子属性列名</summary>
public static class DeliveryColumns
{
    /// <summary>领取次数（人工重试时清零）</summary>
    public const string Attempts = nameof(Attempts);

    /// <summary>下次可领取时间</summary>
    public const string NextAttemptAt = nameof(NextAttemptAt);

    /// <summary>投递任务的领取令牌</summary>
    public const string LeaseToken = nameof(LeaseToken);

    /// <summary>租约到期时间</summary>
    public const string LeaseUntil = nameof(LeaseUntil);

    /// <summary>渠道返回的消息标识</summary>
    public const string ProviderMessageId = nameof(ProviderMessageId);

    /// <summary>最近一次失败原因</summary>
    public const string LastError = nameof(LastError);

    /// <summary>发送成功时间</summary>
    public const string SentAt = nameof(SentAt);

    /// <summary>放弃时间（未放弃时为空）</summary>
    public const string AbandonedAt = nameof(AbandonedAt);

    /// <summary>放弃原因</summary>
    public const string AbandonReason = nameof(AbandonReason);
}
