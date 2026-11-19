using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 强制下线的请求
/// </summary>
/// <param name="SessionId">目标会话 ID</param>
public sealed record ForceLogoutRequest(SessionId? SessionId);
