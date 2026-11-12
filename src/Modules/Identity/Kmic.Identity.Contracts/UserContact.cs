namespace Kmic.Identity.Contracts;

/// <summary>
/// 一个用户的联系方式
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="DisplayName">姓名</param>
/// <param name="Email">邮箱，没有填写时为空</param>
/// <param name="Mobile">手机号，国际格式，没有填写时为空</param>
/// <param name="IsActive">用户是否启用；已禁用的用户不再收到通知</param>
public sealed record UserContact(Guid UserId, string DisplayName, string? Email, string? Mobile, bool IsActive);
