namespace Kmic.Identity.Presentation.Roles;

/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="Reason">原因</param>
public sealed record DisableRoleRequest(long ExpectedVersion, string? Reason);
