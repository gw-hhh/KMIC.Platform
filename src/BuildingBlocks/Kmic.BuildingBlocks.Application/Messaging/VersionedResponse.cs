using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>修改操作返回值，承载记录 ID 与保存后的并发版本号。</summary>
public sealed record VersionedResponse<TId>(TId Id, [property: JsonPropertyName("version")] long ConcurrencyVersion = 0) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    [JsonIgnore]
    public Guid AggregateId => Id.Value;

    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
