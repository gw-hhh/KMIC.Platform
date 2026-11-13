namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递表里不属于聚合属性的列名
/// </summary>
/// <remarks>
/// 这些列是影子属性，只由投递工作器和管理用例用带条件的语句更新；查询用 EF.Property 读取，列名集中在这里
/// </remarks>
public static class DeliveryColumns
{
    /// <summary>
    /// 已经尝试发送几次
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
