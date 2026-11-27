namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>死信列表筛选条件，各非空条件取交集。</summary>
/// <param name="Box">所属消息箱</param>
/// <param name="Subscription">订阅名</param>
/// <param name="MessageType">消息名称</param>
public sealed record DeadMessageFilter(MessageBox? Box = null, string? Subscription = null, string? MessageType = null)
{
    public static DeadMessageFilter None { get; } = new();
}
