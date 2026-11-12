using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 自己的一个登录设备
/// </summary>
/// <param name="Id">会话 ID</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="DeviceName">设备名，取自登录时的 User-Agent</param>
/// <param name="LoginIp">登录时的 IP</param>
/// <param name="CreatedAt">登录时间</param>
/// <param name="LastRefreshedAt">最近一次刷新令牌的时间</param>
/// <param name="IsCurrent">是不是当前正在用的这个会话</param>
public sealed record MySessionItem(SessionId Id, ClientType ClientType, string? DeviceName, string? LoginIp, DateTimeOffset CreatedAt, DateTimeOffset LastRefreshedAt, bool IsCurrent);
