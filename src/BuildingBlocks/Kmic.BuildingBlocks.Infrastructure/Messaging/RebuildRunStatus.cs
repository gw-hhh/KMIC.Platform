namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 重建轮次的状态取值
/// </summary>
/// <remarks>
/// 存为 smallint，唯一索引和 SQL 直接按数字写条件，取值不能改
/// </remarks>
internal static class RebuildRunStatus
{
    /// <summary>
    /// 排队或进行中
    /// </summary>
    public const short Active = 0;

    /// <summary>
    /// 源端没找到的行太多，暂停等运维确认
    /// </summary>
    public const short AwaitingConfirmation = 1;

    public const short Completed = 2;

    public const short Abandoned = 3;

    /// <summary>
    /// 作业失败或被取消，轮次也跟着结束
    /// </summary>
    public const short Failed = 4;
}
