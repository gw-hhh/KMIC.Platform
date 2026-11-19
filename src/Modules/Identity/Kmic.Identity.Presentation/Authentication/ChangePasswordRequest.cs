namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 修改密码的请求
/// </summary>
/// <param name="OldPassword">旧密码</param>
/// <param name="NewPassword">新密码</param>
public sealed record ChangePasswordRequest(string? OldPassword, string NewPassword);
