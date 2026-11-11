namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 一次投递的结果
/// </summary>
internal enum DeliveryResult
{
    /// <summary>
    /// 已写入全部订阅方并标记已投递
    /// </summary>
    Delivered,

    /// <summary>
    /// 占用已被别的投递者接管，本次放弃
    /// </summary>
    TakenOver,

    /// <summary>
    /// 失败，稍后重试
    /// </summary>
    Failed,

    /// <summary>
    /// 失败次数用完，进入死信
    /// </summary>
    Dead,
}
