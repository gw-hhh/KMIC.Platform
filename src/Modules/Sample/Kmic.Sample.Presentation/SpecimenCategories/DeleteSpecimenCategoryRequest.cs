namespace Kmic.Sample.Presentation.SpecimenCategories;

/// <summary>
/// 删除示例分类的请求
/// </summary>
/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record DeleteSpecimenCategoryRequest(long ExpectedVersion);
