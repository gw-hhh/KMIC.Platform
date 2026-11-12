using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 创建结果
/// </summary>
/// <remarks>
/// 新数据本身就是聚合根时只给 ID，保存后的版本号取这个聚合根的。新数据是别的聚合根里的子实体
/// （比如组织树里的一个单元）时，版本号要取它所属的聚合根的，用 <paramref name="OwnerId" /> 指明
/// </remarks>
/// <param name="Id">新数据的 ID</param>
/// <param name="ConcurrencyVersion">保存后的版本号，输出为 version</param>
/// <param name="OwnerId">新数据所属的聚合根的 ID；新数据自己就是聚合根时不给</param>
public sealed record CreatedResponse<TId>(TId Id, [property: JsonPropertyName("version")] long ConcurrencyVersion = 0, [property: JsonIgnore] Guid? OwnerId = null) : IHasConcurrencyVersion
    where TId : struct, IStronglyTypedId
{
    /// <inheritdoc />
    [JsonIgnore]
    public Guid AggregateId => OwnerId ?? Id.Value;

    /// <inheritdoc />
    public IHasConcurrencyVersion WithConcurrencyVersion(long version) => this with { ConcurrencyVersion = version };
}
