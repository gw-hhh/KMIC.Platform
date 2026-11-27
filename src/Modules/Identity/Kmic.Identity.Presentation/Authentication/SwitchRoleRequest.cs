using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Authentication;

/// <param name="RoleId">目标角色 ID</param>
public sealed record SwitchRoleRequest(RoleId RoleId);
