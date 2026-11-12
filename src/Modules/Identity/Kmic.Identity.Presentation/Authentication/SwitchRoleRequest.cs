using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 切换角色的请求
/// </summary>
/// <param name="RoleId">要切换到的角色</param>
public sealed record SwitchRoleRequest(RoleId RoleId);
