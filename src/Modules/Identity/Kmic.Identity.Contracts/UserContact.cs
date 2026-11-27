namespace Kmic.Identity.Contracts;

/// <param name="UserId">用户 ID</param>
/// <param name="DisplayName">姓名</param>
/// <param name="Email">邮箱，未填写时为空</param>
/// <param name="Mobile">手机号，未填写时为空</param>
/// <param name="IsActive">用户是否启用</param>
public sealed record UserContact(Guid UserId, string DisplayName, string? Email, string? Mobile, bool IsActive);
