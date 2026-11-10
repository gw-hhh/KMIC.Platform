namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合内部不变量被破坏：理论上不会发生，发生即程序缺陷
/// </summary>
/// <remarks>
/// 领域层唯一允许显式抛出的自定义异常；业务失败一律返回 Result
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
