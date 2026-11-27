namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

// 后台作业状态取值（存为 smallint，SQL 直接按数字写，取值不可改）
internal static class JobStatus
{
    public const short Queued = 0;

    public const short Running = 1;

    public const short Succeeded = 2;

    public const short Failed = 3;

    public const short Cancelling = 4;

    public const short Cancelled = 5;
}
