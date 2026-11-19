namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 提交结果未知
/// </summary>
/// <remarks>
/// 业务可能已生效也可能没有，不能当成功或回滚处理
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
