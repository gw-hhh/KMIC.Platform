namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 唤醒投递和处理的通知频道，通知内容是模块名
/// </summary>
internal static class MessagingChannels
{
    public const string Outbox = "kmic_outbox";

    public const string Inbox = "kmic_inbox";
}
