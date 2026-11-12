namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>
/// 用户可用的一个角色
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="Name">角色名称</param>
public sealed record MeRole(Guid Id, string Name);
