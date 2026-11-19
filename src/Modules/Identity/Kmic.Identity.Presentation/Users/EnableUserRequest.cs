namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 启用用户的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record EnableUserRequest(long ExpectedVersion);
