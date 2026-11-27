namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>容量保护拒绝消息写入时抛出（命令管道将其转换为 503 Common.MessagingSuspended 并回滚事务）</summary>
public sealed class MessagingSuspendedException : Exception
{
    public MessagingSuspendedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MessagingSuspendedException()
    {
    }

    public MessagingSuspendedException(string message)
        : base(message)
    {
    }
}
