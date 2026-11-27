namespace Kmic.BuildingBlocks.Application.Integration;

public enum MessageBox
{
    /// <summary>发件箱，存储本模块发布的待投递消息。</summary>
    Outbox = 0,

    /// <summary>收件箱，按订阅维度存储已接收的待处理消息。</summary>
    Inbox = 1,
}
