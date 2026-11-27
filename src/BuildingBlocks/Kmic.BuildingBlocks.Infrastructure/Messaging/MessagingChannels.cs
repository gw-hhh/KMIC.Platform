namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// PG LISTEN/NOTIFY 频道名（Outbox、Inbox、Jobs 频道的载荷均为模块名）
internal static class MessagingChannels
{
    public const string Outbox = "kmic_outbox";

    public const string Inbox = "kmic_inbox";

    public const string Jobs = "kmic_jobs";

    public const string Cache = "kmic_cache";
}
