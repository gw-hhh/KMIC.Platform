namespace Spike02.Domain;

/// <summary>
/// 有归属人的聚合（05-05 第 5.2 节）
/// </summary>
public interface IHasOwner
{
    UserId OwnerId { get; }
}
