using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <param name="SessionId">目标会话 ID</param>
public sealed record ForceLogoutRequest(SessionId? SessionId);
