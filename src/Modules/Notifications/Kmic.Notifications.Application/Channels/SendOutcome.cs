namespace Kmic.Notifications.Application.Channels;

/// <summary>单次发送结果类别</summary>
public enum SendOutcome
{
    /// <summary>渠道已受理发送请求（不代表收件人已收到）</summary>
    Sent,

    /// <summary>暂时失败（网络、限流或渠道繁忙），稍后重试可能成功</summary>
    Transient,

    /// <summary>永久失败（如地址或模板无效），不再自动重试</summary>
    Permanent,
}
