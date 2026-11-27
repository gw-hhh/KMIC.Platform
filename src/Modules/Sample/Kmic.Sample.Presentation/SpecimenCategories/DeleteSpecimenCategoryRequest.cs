namespace Kmic.Sample.Presentation.SpecimenCategories;

/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record DeleteSpecimenCategoryRequest(long ExpectedVersion);
