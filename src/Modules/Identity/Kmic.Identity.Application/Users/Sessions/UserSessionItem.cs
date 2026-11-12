using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>
/// 用户的一个有效会话
/// </summary>
/// <param name="Id">会话 ID</param>
/// <param name="UserId">会话属于哪个用户；模拟会话属于被模拟的人</param>
/// <param name="ImpersonatorUserId">模拟会话里实际操作的人；普通会话为空</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="DeviceName">设备名，取自登录时的 User-Agent</param>
/// <param name="LoginIp">登录时的 IP</param>
/// <param name="CreatedAt">登录时间</param>
/// <param name="LastRefreshedAt">最近一次刷新令牌的时间</param>
/// <param name="RefreshExpiresAt">不再刷新就在什么时候失效</param>
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
