namespace Kmic.Identity.Presentation.Users;

/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="Reason">原因</param>
public sealed record DisableUserRequest(long ExpectedVersion, string? Reason);
