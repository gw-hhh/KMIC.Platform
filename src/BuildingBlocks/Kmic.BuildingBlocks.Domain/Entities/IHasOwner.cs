using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有所属人的数据
/// </summary>
public interface IHasOwner
{
    /// <summary>
    /// 所属人
    /// </summary>
    UserId OwnerId { get; }
}
