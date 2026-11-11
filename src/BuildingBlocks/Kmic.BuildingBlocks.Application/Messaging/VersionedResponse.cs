using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 修改类命令的收据：聚合的 ID 与保存后的版本，前端下次修改时带回
/// </summary>
/// <param name="Id">聚合的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本，由事务行为回填</param>
public sealed record VersionedResponse<TId>(TId Id, long ConcurrencyVersion = 0) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    public Guid AggregateId => Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
