using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>标识数据归属于特定用户。</summary>
public interface IHasOwner
{
    /// <summary>所属用户 ID。</summary>
    UserId OwnerId { get; }
}
