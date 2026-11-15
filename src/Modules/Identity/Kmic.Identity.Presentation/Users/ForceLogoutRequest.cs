using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 强制下线的请求
/// </summary>
/// <param name="SessionId">目标会话 ID；未指定时结束该用户的全部未作废会话，包括以其身份创建或由其发起的模拟会话</param>
public sealed record ForceLogoutRequest(SessionId? SessionId);
