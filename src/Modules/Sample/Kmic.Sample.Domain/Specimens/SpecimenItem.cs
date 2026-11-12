using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本的明细
/// </summary>
/// <remarks>
/// 聚合内的子实体：只能通过聚合根创建和修改，随聚合一起保存
/// </remarks>
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
