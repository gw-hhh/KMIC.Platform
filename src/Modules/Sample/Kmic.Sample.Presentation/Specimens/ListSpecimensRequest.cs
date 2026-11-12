namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 查询示例标本列表的参数
/// </summary>
/// <param name="Q">关键字，在编号和标题里模糊匹配</param>
/// <param name="Status">状态编码，重复参数名可以给多个，如 status=Draft&amp;status=Submitted</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -createdAt,number：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段</param>
public sealed record ListSpecimensRequest(string? Q, string[]? Status, int Page = 1, int PageSize = 20, string? Sort = null);
