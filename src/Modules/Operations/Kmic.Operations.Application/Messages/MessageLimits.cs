namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 消息管理的批量上限
/// </summary>
internal static class MessageLimits
{
    /// <summary>
    /// 一次重试或放弃最多多少条
    /// </summary>
    public const int MaxPerRequest = 200;
}
