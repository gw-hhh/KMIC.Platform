namespace Kmic.Sample.Presentation.SpecimenCategories;

/// <summary>
/// 新建示例分类的请求
/// </summary>
/// <param name="Name">名称</param>
public sealed record CreateSpecimenCategoryRequest(string Name);
