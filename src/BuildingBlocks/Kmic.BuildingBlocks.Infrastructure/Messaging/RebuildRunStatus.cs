namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 读模型重建轮次状态（存为 smallint，SQL 直接按数字写，取值不可改）
internal static class RebuildRunStatus
{
    // 排队或进行中
    public const short Active = 0;

    // 源端缺失行数超限，暂停等运维确认
    public const short AwaitingConfirmation = 1;

    public const short Completed = 2;

    public const short Abandoned = 3;

    // 作业失败或被取消，对应轮次随之结束
    public const short Failed = 4;
}
