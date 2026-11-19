using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例的明细，由示例创建、随示例一起保存
/// </summary>
public sealed class SpecimenItem : Entity<SpecimenItemId>
{
    private SpecimenItem(SpecimenItemId id, string name, int quantity)
        : base(id)
    {
        Name = name;
        Quantity = quantity;
    }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; private set; }

    internal static SpecimenItem Create(string name, int quantity) => new(SpecimenItemId.New(), name, quantity);
}
