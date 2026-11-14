using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 切换角色的请求
/// </summary>
/// <param name="RoleId">目标角色 ID</param>
public sealed record SwitchRoleRequest(RoleId RoleId);
