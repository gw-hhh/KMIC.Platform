using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>用户的一个有效会话。</summary>
/// <param name="Id">会话 ID</param>
/// <param name="UserId">会话所属用户 ID</param>
/// <param name="ImpersonatorUserId">实际操作人 ID，仅模拟会话有值，普通会话为空</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="DeviceName">设备名</param>
/// <param name="LoginIp">登录时的 IP</param>
/// <param name="CreatedAt">登录时间</param>
/// <param name="LastRefreshedAt">最近刷新时间</param>
/// <param name="RefreshExpiresAt">刷新令牌到期时间</param>
public sealed record UserSessionItem(
    SessionId Id,
    UserId UserId,
    UserId? ImpersonatorUserId,
    ClientType ClientType,
    string? DeviceName,
    string? LoginIp,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastRefreshedAt,
    DateTimeOffset RefreshExpiresAt);
