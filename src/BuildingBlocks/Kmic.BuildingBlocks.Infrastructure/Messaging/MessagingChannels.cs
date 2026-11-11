namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 唤醒投递、处理和作业执行的通知频道，通知内容是模块名
/// </summary>
internal static class MessagingChannels
{
    public const string Outbox = "kmic_outbox";

    public const string Inbox = "kmic_inbox";

    public const string Jobs = "kmic_jobs";

    public const string Cache = "kmic_cache";
}
