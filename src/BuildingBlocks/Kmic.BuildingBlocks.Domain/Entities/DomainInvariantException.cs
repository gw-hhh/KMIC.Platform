namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 领域不变量被破坏
/// </summary>
/// <remarks>
/// 只用于程序缺陷，业务失败要返回 Result
/// </remarks>
public sealed class DomainInvariantException : Exception
{
    /// <summary>
    /// 无说明的实例
    /// </summary>
    public DomainInvariantException()
    {
    }

    /// <summary>
    /// 带说明的实例
    /// </summary>
    public DomainInvariantException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 带说明与内部异常的实例
    /// </summary>
    public DomainInvariantException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
