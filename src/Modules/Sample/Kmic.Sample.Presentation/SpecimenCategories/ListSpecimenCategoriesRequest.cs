namespace Kmic.Sample.Presentation.SpecimenCategories;

/// <summary>
/// 查询示例分类列表的参数
/// </summary>
/// <param name="Q">名称关键字</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListSpecimenCategoriesRequest(string? Q, int Page = 1, int PageSize = 20, string? Sort = null);
