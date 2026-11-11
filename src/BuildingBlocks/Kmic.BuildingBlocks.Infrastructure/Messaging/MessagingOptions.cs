namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 集成消息配置
/// </summary>
public sealed class MessagingOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Messaging";

    /// <summary>
    /// 单条消息内容的上限（字节）
    /// </summary>
    public int MaxPayloadBytes { get; set; } = 256 * 1024;

    /// <summary>
    /// 一个事务写入 Outbox 的上限（字节）
    /// </summary>
    public int MaxOutboxBytesPerTransaction { get; set; } = 1024 * 1024;
}
