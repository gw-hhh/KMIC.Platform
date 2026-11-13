using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 管理视图里一条消息的状态：只有运维关心的两种
/// </summary>
public sealed class MessageState : Enumeration<MessageState>
{
    /// <summary>
    /// 死信：可以重试或放弃
    /// </summary>
    public static readonly MessageState Dead = new("Dead", "死信");

    /// <summary>
    /// 已被运维放弃，不再处理
    /// </summary>
    public static readonly MessageState Abandoned = new("Abandoned", "已放弃");

    private MessageState(string code, string name)
        : base(code, name)
    {
    }
}
