namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 选人下拉的参数
/// </summary>
/// <param name="Q">关键字，在姓名、英文名、工号、账号里模糊匹配</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
public sealed record SearchUserOptionsRequest(string? Q, int Page = 1, int PageSize = 20);
