namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 实体基类：同一运行时类型且 Id 相同即相等
/// </summary>
/// <remarks>
/// ID 在创建时就生成（UUID v7），不存在“未保存时 Id 为空”的实体，相等性不需要为临时实体特殊处理
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// 用已生成的 ID 创建
    /// </summary>
    protected Entity(TId id) => Id = id;

    /// <summary>
    /// 只供 EF 物化使用，派生类以私有无参构造函数调用
    /// </summary>
    protected Entity() => Id = default!;

    /// <summary>
    /// 实体标识
    /// </summary>
    public TId Id { get; protected init; }

    /// <summary>
    /// 相等
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    /// <summary>
    /// 不相等
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other) =>
        other is not null
        && (ReferenceEquals(this, other) || (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id)));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
