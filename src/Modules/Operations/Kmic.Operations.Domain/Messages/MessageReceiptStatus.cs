using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 重试或放弃一条消息的结果
/// </summary>
public sealed class MessageReceiptStatus : Enumeration<MessageReceiptStatus>
{
    /// <summary>
    /// 已经做了
    /// </summary>
    public static readonly MessageReceiptStatus Done = new("Done", "已处理");

    /// <summary>
    /// 这个模块的这个箱里没有这一行
    /// </summary>
    public static readonly MessageReceiptStatus NotFound = new("NotFound", "不存在");

    /// <summary>
    /// 这一行不是死信：已经处理完、正在处理，或者早就放弃了，没有改它
    /// </summary>
    public static readonly MessageReceiptStatus WrongState = new("WrongState", "状态不对");

    /// <summary>
    /// 接收方的放弃检查不让放弃，原因见回执；没有改它
    /// </summary>
    public static readonly MessageReceiptStatus Refused = new("Refused", "被拒绝");

    private MessageReceiptStatus(string code, string name)
        : base(code, name)
    {
    }
}
