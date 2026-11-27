using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>重试或放弃消息的操作结果。</summary>
public sealed class MessageReceiptStatus : Enumeration<MessageReceiptStatus>
{
    public static readonly MessageReceiptStatus Done = new("Done", "已处理");

    /// <summary>指定模块的收发件箱中不存在该消息。</summary>
    public static readonly MessageReceiptStatus NotFound = new("NotFound", "不存在");

    /// <summary>消息当前不是死信，未执行操作。</summary>
    public static readonly MessageReceiptStatus WrongState = new("WrongState", "状态不符");

    /// <summary>接收方拒绝放弃消息（状态不变，原因见返回结果）。</summary>
    public static readonly MessageReceiptStatus Refused = new("Refused", "被拒绝");

    private MessageReceiptStatus(string code, string name)
        : base(code, name)
    {
    }
}
