namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 消息所在的箱：发出去的在发件箱，收到的在收件箱
/// </summary>
public enum MessageBox
{
    /// <summary>
    /// 发件箱：本模块发布的消息，等投递程序送到订阅方的收件箱
    /// </summary>
    Outbox = 0,

    /// <summary>
    /// 收件箱：一条消息对一个订阅一行，等处理程序执行订阅映射的命令
    /// </summary>
    Inbox = 1,
}
