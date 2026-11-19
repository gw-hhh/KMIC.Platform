namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 新建用户的请求
/// </summary>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
public sealed record CreateUserRequest(string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email);
