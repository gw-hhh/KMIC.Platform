namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal enum DeliveryResult
{
    Delivered,

    // 租约已被其他投递者接管，放弃本次投递
    TakenOver,

    Failed,

    // 重试耗尽，已转入死信
    Dead,
}
