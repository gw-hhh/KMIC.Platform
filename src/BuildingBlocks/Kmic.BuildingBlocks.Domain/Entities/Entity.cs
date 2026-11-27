namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>实体基类，依据运行时类型与 ID 判定相等性。</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>使用既有 ID 初始化实体。</summary>
    protected Entity(TId id) => Id = id;

    /// <summary>仅供 EF Core 实例化使用。</summary>
    protected Entity() => Id = default!;

    /// <summary>
    /// ID
    /// </summary>
    public TId Id { get; protected init; }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);

    public bool Equals(Entity<TId>? other) =>
        other is not null
        && (ReferenceEquals(this, other) || (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id)));

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
