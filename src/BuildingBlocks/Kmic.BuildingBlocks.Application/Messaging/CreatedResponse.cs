using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>创建操作返回值；若新建对象为子实体，须通过 <paramref name="OwnerId" /> 指定所属聚合根以回填并发版本号。</summary>
public sealed record CreatedResponse<TId>(TId Id, [property: JsonPropertyName("version")] long ConcurrencyVersion = 0, [property: JsonIgnore] Guid? OwnerId = null) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    [JsonIgnore]
    public Guid AggregateId => OwnerId ?? Id.Value;

    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
