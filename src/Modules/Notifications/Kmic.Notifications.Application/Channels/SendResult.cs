namespace Kmic.Notifications.Application.Channels;

/// <summary>渠道单条发送结果</summary>
/// <param name="Outcome">结果类别</param>
/// <param name="ProviderMessageId">渠道消息标识（成功时记录）</param>
/// <param name="Error">失败原因</param>
public sealed record SendResult(SendOutcome Outcome, string? ProviderMessageId = null, string? Error = null)
{
    public static SendResult Sent(string? providerMessageId = null) => new(SendOutcome.Sent, providerMessageId);

    public static SendResult Transient(string error) => new(SendOutcome.Transient, Error: error);

    public static SendResult Permanent(string error) => new(SendOutcome.Permanent, Error: error);
}
