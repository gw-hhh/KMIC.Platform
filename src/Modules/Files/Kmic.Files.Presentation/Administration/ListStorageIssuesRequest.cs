namespace Kmic.Files.Presentation.Administration;

/// <param name="Status">状态编码</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListStorageIssuesRequest(string[]? Status, int Page = 1, int PageSize = 20, string? Sort = null);
