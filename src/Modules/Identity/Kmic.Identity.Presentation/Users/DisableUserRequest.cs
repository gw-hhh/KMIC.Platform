namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 禁用用户的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Reason">原因，记入状态历史</param>
public sealed record DisableUserRequest(long ExpectedVersion, string? Reason);
