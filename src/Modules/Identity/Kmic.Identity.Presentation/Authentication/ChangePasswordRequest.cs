namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 修改密码的请求
/// </summary>
/// <param name="OldPassword">旧密码；还没有设置过密码时不用给</param>
/// <param name="NewPassword">新密码</param>
public sealed record ChangePasswordRequest(string? OldPassword, string NewPassword);
