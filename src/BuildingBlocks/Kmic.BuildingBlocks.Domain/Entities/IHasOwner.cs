using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有所属人的资源；所属人的业务含义由模块在聚合注释中写明
/// </summary>
public interface IHasOwner
{
    /// <summary>
    /// 所属人
    /// </summary>
    UserId OwnerId { get; }
}
