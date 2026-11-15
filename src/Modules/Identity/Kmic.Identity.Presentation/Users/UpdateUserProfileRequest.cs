namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 修改用户资料的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回 409 Common.VersionConflict</param>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
public sealed record UpdateUserProfileRequest(long ExpectedVersion, string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email);
