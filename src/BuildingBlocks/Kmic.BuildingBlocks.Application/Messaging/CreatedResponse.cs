using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 创建结果
/// </summary>
/// <param name="Id">新数据的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本号</param>
public sealed record CreatedResponse<TId>(TId Id, long ConcurrencyVersion = 0) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    public Guid AggregateId => Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
