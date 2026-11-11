namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 实体基类
/// </summary>
/// <remarks>
/// 类型和 ID 都相同即相等
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// 用已生成的 ID 创建
    /// </summary>
    protected Entity(TId id) => Id = id;

    /// <summary>
    /// 只供 EF 使用
    /// </summary>
    protected Entity() => Id = default!;

    /// <summary>
    /// ID
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
