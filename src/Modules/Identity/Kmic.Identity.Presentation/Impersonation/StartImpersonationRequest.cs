using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Impersonation;

/// <param name="RoleId">目标用户的角色 ID</param>
/// <param name="Reason">原因</param>
public sealed record StartImpersonationRequest(RoleId? RoleId, string Reason);
