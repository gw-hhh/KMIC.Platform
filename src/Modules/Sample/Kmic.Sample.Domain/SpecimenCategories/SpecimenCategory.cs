using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Sample.Domain.SpecimenCategories;

/// <summary>示例分类（简单维护档示例，仅含名称与启用状态）</summary>
public sealed class SpecimenCategory : AggregateRoot<SpecimenCategoryId>
{
    private SpecimenCategory(SpecimenCategoryId id, string name)
        : base(id)
    {
        Name = name;
    }

    /// <summary>
    /// 名称，唯一
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>新建示例分类（默认启用）</summary>
    public static SpecimenCategory Create(string name) => new(SpecimenCategoryId.New(), name);

    public void Update(string name, bool isEnabled)
    {
        Name = name;
        IsEnabled = isEnabled;
    }
}
