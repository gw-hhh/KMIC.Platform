namespace Kmic.Sample.Presentation.SpecimenCategories;

/// <param name="Name">名称</param>
/// <param name="IsEnabled">是否启用</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record UpdateSpecimenCategoryRequest(string Name, bool IsEnabled, long ExpectedVersion);
