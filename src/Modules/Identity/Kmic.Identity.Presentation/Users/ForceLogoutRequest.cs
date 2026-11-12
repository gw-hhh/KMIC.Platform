using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 强制下线的请求
/// </summary>
/// <param name="SessionId">要下线的会话；不给就是该用户的全部会话</param>
public sealed record ForceLogoutRequest(SessionId? SessionId);
