namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// Outbox、Inbox 状态取值（存为 smallint，SQL 直接按数字写，取值不可改）
internal static class MessageStatus
{
    public const short Pending = 0;

    public const short Processing = 1;

    public const short Done = 2;

    public const short Dead = 3;

    // 只有 Inbox 有
    public const short Abandoned = 4;
}
