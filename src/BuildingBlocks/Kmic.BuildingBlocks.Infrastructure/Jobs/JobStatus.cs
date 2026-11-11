namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 后台作业的状态取值
/// </summary>
/// <remarks>
/// 存为 smallint，领取和查询 SQL 直接按数字写条件，取值不能改
/// </remarks>
internal static class JobStatus
{
    public const short Queued = 0;

    public const short Running = 1;

    public const short Succeeded = 2;

    public const short Failed = 3;

    public const short Cancelling = 4;

    public const short Cancelled = 5;
}
