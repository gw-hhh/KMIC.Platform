namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>领域不变量被破坏时抛出的异常（仅用于程序缺陷，业务失败应返回 Result）。</summary>
public sealed class DomainInvariantException : Exception
{
    public DomainInvariantException()
    {
    }

    public DomainInvariantException(string message)
        : base(message)
    {
    }

    public DomainInvariantException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
