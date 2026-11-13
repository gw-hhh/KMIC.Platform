using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 消息所在的箱
/// </summary>
/// <remarks>
/// 编码就是框架里消息箱枚举的成员名；领域层不引用应用层，所以单独定义，由应用层转换
/// </remarks>
public sealed class MessageBoxKind : Enumeration<MessageBoxKind>
{
    /// <summary>
    /// 发件箱：本模块发布的消息，等投递程序送到订阅方
    /// </summary>
    public static readonly MessageBoxKind Outbox = new("Outbox", "发件箱");

    /// <summary>
    /// 收件箱：一条消息对一个订阅一行，等处理程序执行
    /// </summary>
    public static readonly MessageBoxKind Inbox = new("Inbox", "收件箱");

    private MessageBoxKind(string code, string name)
        : base(code, name)
    {
    }
}
