using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 授予上的一个锚点
/// </summary>
/// <param name="OrgUnitId">组织单元 ID</param>
/// <param name="Name">组织单元的名称</param>
public sealed record UserAnchorItem(OrgUnitId OrgUnitId, string? Name);
