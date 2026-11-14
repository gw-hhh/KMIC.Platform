namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 禁用用户的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Reason">原因，记入状态历史</param>
public sealed record DisableUserRequest(long ExpectedVersion, string? Reason);
