namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal enum InboxResult
{
    // 业务修改与消息完成状态已随事务一并提交
    Processed,

    // 租约已被其他处理者接管，本次修改已回滚
    TakenOver,

    Failed,

    Dead,

    // 回写状态时行已丢失或无法读取，交由租约过期后重新领取
    Unknown,
}
