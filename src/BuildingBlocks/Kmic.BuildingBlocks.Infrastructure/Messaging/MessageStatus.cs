namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Outbox、Inbox 的状态取值
/// </summary>
/// <remarks>
/// 存为 smallint，领取和运维 SQL 直接按数字写条件，取值不能改
/// </remarks>
internal static class MessageStatus
{
    public const short Pending = 0;

    public const short Processing = 1;

    public const short Done = 2;

    public const short Dead = 3;

    // 只有 Inbox 有
    public const short Abandoned = 4;
}
