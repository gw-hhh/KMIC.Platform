namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 消息管理的批量上限
/// </summary>
internal static class MessageLimits
{
    /// <summary>
    /// 单次重试或放弃的消息数上限
    /// </summary>
    public const int MaxPerRequest = 200;
}
