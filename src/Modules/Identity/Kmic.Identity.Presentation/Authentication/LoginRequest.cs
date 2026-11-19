namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 登录请求
/// </summary>
/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型</param>
public sealed record LoginRequest(string Account, string Password, string ClientType);
