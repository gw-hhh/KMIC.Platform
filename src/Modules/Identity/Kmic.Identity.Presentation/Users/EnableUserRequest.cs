namespace Kmic.Identity.Presentation.Users;

/// <param name="ExpectedVersion">预期版本号</param>
public sealed record EnableUserRequest(long ExpectedVersion);
