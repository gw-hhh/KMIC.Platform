using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 创建结果
/// </summary>
/// <param name="Id">新数据的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本号，输出为 version</param>
public sealed record CreatedResponse<TId>(TId Id, [property: JsonPropertyName("version")] long ConcurrencyVersion = 0) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    [JsonIgnore]
    public Guid AggregateId => Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
