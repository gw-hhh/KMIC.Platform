namespace Kmic.Identity.Presentation.Users;

/// <param name="Q">关键字</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
public sealed record SearchUserOptionsRequest(string? Q, int Page = 1, int PageSize = 20);
