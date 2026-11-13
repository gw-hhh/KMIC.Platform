namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 渠道发送一条消息的结果
/// </summary>
/// <param name="Outcome">结果类别</param>
/// <param name="ProviderMessageId">渠道返回的消息标识，发送成功时记下</param>
/// <param name="Error">失败原因，会写进投递记录，不要带收件人的联系方式</param>
public sealed record SendResult(SendOutcome Outcome, string? ProviderMessageId = null, string? Error = null)
{
    /// <summary>
    /// 已发送
    /// </summary>
    public static SendResult Sent(string? providerMessageId = null) => new(SendOutcome.Sent, providerMessageId);

    /// <summary>
    /// 暂时失败，稍后重试
    /// </summary>
    public static SendResult Transient(string error) => new(SendOutcome.Transient, Error: error);

    /// <summary>
    /// 永久失败，不再重试
    /// </summary>
    public static SendResult Permanent(string error) => new(SendOutcome.Permanent, Error: error);
}
