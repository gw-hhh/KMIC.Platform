using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 本人的登录会话及设备信息
/// </summary>
/// <param name="Id">会话 ID</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="DeviceName">设备名</param>
/// <param name="LoginIp">登录时的 IP</param>
/// <param name="CreatedAt">登录时间</param>
/// <param name="LastRefreshedAt">最近刷新时间</param>
/// <param name="IsCurrent">是否当前会话</param>
public sealed record MySessionItem(SessionId Id, ClientType ClientType, string? DeviceName, string? LoginIp, DateTimeOffset CreatedAt, DateTimeOffset LastRefreshedAt, bool IsCurrent);
