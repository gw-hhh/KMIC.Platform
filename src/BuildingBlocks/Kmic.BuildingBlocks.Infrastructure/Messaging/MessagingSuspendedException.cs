namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 容量保护拒绝了消息写入，业务事务随之回滚
/// </summary>
/// <remarks>
/// 命令管道把它转成可重试的 503 <c>Common.MessagingSuspended</c>
/// </remarks>
public sealed class MessagingSuspendedException : Exception
{
    /// <summary>
    /// 带说明和原因的实例
    /// </summary>
    public MessagingSuspendedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 无说明的实例
    /// </summary>
    public MessagingSuspendedException()
    {
    }

    /// <summary>
    /// 带说明的实例
    /// </summary>
    public MessagingSuspendedException(string message)
        : base(message)
    {
    }
}
