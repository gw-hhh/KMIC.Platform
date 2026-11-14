namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 查询存储问题的参数
/// </summary>
/// <param name="Status">状态编码，重复参数名可以给多个，如 status=Open</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -foundAt</param>
public sealed record ListStorageIssuesRequest(string[]? Status, int Page = 1, int PageSize = 20, string? Sort = null);
