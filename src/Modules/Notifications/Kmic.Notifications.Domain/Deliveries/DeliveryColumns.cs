namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递表影子属性的列名
/// </summary>
public static class DeliveryColumns
{
    /// <summary>
    /// 领取次数，人工重试时清零
    /// </summary>
    public const string Attempts = nameof(Attempts);

    /// <summary>
    /// 下次可以领取的时间
    /// </summary>
    public const string NextAttemptAt = nameof(NextAttemptAt);

    /// <summary>
    /// 投递工作器的领取令牌
    /// </summary>
    public const string LeaseToken = nameof(LeaseToken);

    /// <summary>
    /// 领取的到期时间
    /// </summary>
    public const string LeaseUntil = nameof(LeaseUntil);

    /// <summary>
    /// 渠道返回的消息标识
    /// </summary>
    public const string ProviderMessageId = nameof(ProviderMessageId);

    /// <summary>
    /// 最近一次失败的原因
    /// </summary>
    public const string LastError = nameof(LastError);

    /// <summary>
    /// 发送成功的时间
    /// </summary>
    public const string SentAt = nameof(SentAt);

    /// <summary>
    /// 放弃处理的时间，没有放弃为空
    /// </summary>
    public const string AbandonedAt = nameof(AbandonedAt);

    /// <summary>
    /// 放弃处理的原因
    /// </summary>
    public const string AbandonReason = nameof(AbandonReason);
}
