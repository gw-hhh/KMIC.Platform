using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 创建类命令的收据：新聚合的 ID 与版本，接口层据此返回 201 与 Location
/// </summary>
/// <param name="Id">新聚合的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本，由事务行为回填</param>
public sealed record CreatedResponse<TId>(TId Id, long ConcurrencyVersion = 0) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    public Guid AggregateId => Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
