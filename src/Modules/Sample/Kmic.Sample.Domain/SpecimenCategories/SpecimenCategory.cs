using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Sample.Domain.SpecimenCategories;

/// <summary>
/// 示例分类
/// </summary>
/// <remarks>
/// 简单维护档的示例：只有名称和启用状态，按 ID 增删改
/// </remarks>
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

    /// <summary>
    /// 新建，默认启用
    /// </summary>
    public static SpecimenCategory Create(string name) => new(SpecimenCategoryId.New(), name);

    /// <summary>
    /// 修改名称和启用状态
    /// </summary>
    public void Update(string name, bool isEnabled)
    {
        Name = name;
        IsEnabled = isEnabled;
    }
}
