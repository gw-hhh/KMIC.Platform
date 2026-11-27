using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>收发件箱类型（编码与框架消息箱成员名一致）。</summary>
public sealed class MessageBoxKind : Enumeration<MessageBoxKind>
{
    /// <summary>模块发布的消息，等待投递给订阅方。</summary>
    public static readonly MessageBoxKind Outbox = new("Outbox", "发件箱");

    /// <summary>同一消息在各接收订阅下单独登记，等待处理器执行。</summary>
    public static readonly MessageBoxKind Inbox = new("Inbox", "收件箱");

    private MessageBoxKind(string code, string name)
        : base(code, name)
    {
    }
}
