namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 一次发送的结果类别
/// </summary>
public enum SendOutcome
{
    /// <summary>
    /// 渠道接受了，已发送
    /// </summary>
    Sent,

    /// <summary>
    /// 暂时失败：网络、限流、渠道繁忙，稍后重试可能成功
    /// </summary>
    Transient,

    /// <summary>
    /// 永久失败：地址不合法、模板无效、账号被拒，重试没有意义
    /// </summary>
    Permanent,
}
