using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 创建结果
/// </summary>
/// <remarks>
/// 新数据是子实体时，版本号取所属聚合根的，用 <paramref name="OwnerId" /> 指明
/// </remarks>
/// <param name="Id">新数据的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本号</param>
/// <param name="OwnerId">新数据所属的聚合根的 ID</param>
public sealed record CreatedResponse<TId>(TId Id, [property: JsonPropertyName("version")] long ConcurrencyVersion = 0, [property: JsonIgnore] Guid? OwnerId = null) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    [JsonIgnore]
    public Guid AggregateId => OwnerId ?? Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
