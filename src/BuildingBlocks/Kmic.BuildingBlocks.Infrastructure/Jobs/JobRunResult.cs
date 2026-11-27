namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

internal enum JobRunResult
{
    Succeeded,

    // 错误信息已写入作业行
    Failed,

    Cancelled,

    // 停机时将可续跑作业放回队列，等待其他实例接手
    Released,

    // 租约已被其他执行器接管，本地中止且不写结果
    TakenOver,

    // 批次或最终状态提交结果未知，中止等待租约过期后接管
    OutcomeUnknown,
}
