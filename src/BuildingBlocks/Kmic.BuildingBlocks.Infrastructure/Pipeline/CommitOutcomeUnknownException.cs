namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 提交已经发出、但没有收到确认（如连接在确认返回前断开）：业务可能已经生效，也可能没有
/// </summary>
/// <remarks>
/// 各入口先用自己的记录重新读取判断，判断不了就保持“结果未知”，既不推断成功，也不按回滚处理
/// </remarks>
public sealed class CommitOutcomeUnknownException : Exception
{
    /// <summary>
    /// 包装提交时的异常
    /// </summary>
    public CommitOutcomeUnknownException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 无说明的实例
    /// </summary>
    public CommitOutcomeUnknownException()
    {
    }

    /// <summary>
    /// 带说明的实例
    /// </summary>
    public CommitOutcomeUnknownException(string message)
        : base(message)
    {
    }
}
