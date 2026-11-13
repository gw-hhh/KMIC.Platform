namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 列死信的筛选条件，给了的条件都要满足
/// </summary>
/// <param name="Box">只看发件箱或收件箱</param>
/// <param name="Subscription">订阅名；给了就只看收件箱</param>
/// <param name="MessageType">消息名称</param>
public sealed record DeadMessageFilter(MessageBox? Box = null, string? Subscription = null, string? MessageType = null)
{
    /// <summary>
    /// 不筛选
    /// </summary>
    public static DeadMessageFilter None { get; } = new();
}
