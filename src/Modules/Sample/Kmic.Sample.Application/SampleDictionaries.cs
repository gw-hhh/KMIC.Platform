using Kmic.BuildingBlocks.Application.Dictionaries;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的字典类型
/// </summary>
[DictionaryTypeGroup("Sample")]
public static class SampleDictionaries
{
    /// <summary>
    /// 标本类别：不分层的字典
    /// </summary>
    [DictionaryType("标本类别")]
    public const string Category = "Sample.Category";

    /// <summary>
    /// 产地：分层的字典，示例里用来演示父子项
    /// </summary>
    [DictionaryType("产地", IsHierarchical = true)]
    public const string Origin = "Sample.Origin";
}
