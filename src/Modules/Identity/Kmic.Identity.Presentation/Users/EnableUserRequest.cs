namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 启用用户的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record EnableUserRequest(long ExpectedVersion);
