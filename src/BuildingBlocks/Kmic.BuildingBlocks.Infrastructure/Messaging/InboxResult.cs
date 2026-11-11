namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 处理一条 Inbox 消息的结果
/// </summary>
internal enum InboxResult
{
    /// <summary>
    /// 已处理，业务数据和“已处理”一起提交了
    /// </summary>
    Processed,

    /// <summary>
    /// 占用已被别的处理者接管，本次的修改已回滚
    /// </summary>
    TakenOver,

    /// <summary>
    /// 失败，稍后重试
    /// </summary>
    Failed,

    /// <summary>
    /// 进入死信，等人处理
    /// </summary>
    Dead,

    /// <summary>
    /// 记录结果时发现行已被清理，或者读不出来，交给占用到期后的正常接管
    /// </summary>
    Unknown,
}
