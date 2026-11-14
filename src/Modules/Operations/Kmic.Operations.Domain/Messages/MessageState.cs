using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 消息运维列表的状态
/// </summary>
public sealed class MessageState : Enumeration<MessageState>
{
    /// <summary>
    /// 自动处理已停止，需人工介入
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
