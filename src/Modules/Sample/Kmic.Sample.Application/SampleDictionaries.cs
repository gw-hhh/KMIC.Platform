using Kmic.BuildingBlocks.Application.Dictionaries;

namespace Kmic.Sample.Application;

[DictionaryTypeGroup("Sample")]
public static class SampleDictionaries
{
    [DictionaryType("示例类别")]
    public const string Category = "Sample.Category";

    [DictionaryType("产地", IsHierarchical = true)]
    public const string Origin = "Sample.Origin";
}
