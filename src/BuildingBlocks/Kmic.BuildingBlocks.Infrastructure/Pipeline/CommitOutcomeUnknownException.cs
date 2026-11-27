namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>事务提交结果未知异常，业务事务可能已生效或未生效，调用方不得按确定成功或确定回滚处理</summary>
public sealed class CommitOutcomeUnknownException : Exception
{
    public CommitOutcomeUnknownException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public CommitOutcomeUnknownException()
    {
    }

    public CommitOutcomeUnknownException(string message)
        : base(message)
    {
    }
}
