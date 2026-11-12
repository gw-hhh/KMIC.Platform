namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 修改用户资料的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Account">登录名；改它与改手机号一样，对持有受保护角色的用户只有同样持有受保护角色的人才能做</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
public sealed record UpdateUserProfileRequest(long ExpectedVersion, string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email);
