namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 新建用户的请求
/// </summary>
/// <remarks>
/// 字段名与命令里的同名属性一致，校验错误的路径才能和请求体对上
/// </remarks>
/// <param name="Account">登录名，最多 100 个字符，不区分大小写唯一</param>
/// <param name="DisplayName">姓名，最多 100 个字符</param>
/// <param name="EnglishName">英文名，最多 100 个字符</param>
/// <param name="EmployeeNo">工号，最多 50 个字符，填写时唯一</param>
/// <param name="Mobile">手机号，填写时唯一；国内号码可以不带 +86</param>
/// <param name="Email">邮箱，最多 200 个字符</param>
public sealed record CreateUserRequest(string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email);
