using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Organization;

/// <summary>
/// 组织单元：组织树上的节点
/// </summary>
/// <remarks>
/// 聚合内的子实体：只能通过 OrgTree 创建和修改。旧系统里的“大区负责人”这类对组织的业务描述不放在这里，
/// 组织单元只回答“谁在哪个单元下”，是数据范围的输入
/// </remarks>
public sealed class OrgUnit : Entity<OrgUnitId>
{
    private OrgUnit(OrgUnitId id, OrgUnitId? parentId, string name, string? code, int sortOrder)
        : base(id)
    {
        ParentId = parentId;
        Name = name;
        Code = code;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 上级；根单元为空
    /// </summary>
    public OrgUnitId? ParentId { get; private set; }

    /// <summary>
    /// 名称，同一上级下不重复（不区分大小写）
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 编码，可空；业务上需要稳定标识一个单元时使用
    /// </summary>
    public string? Code { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public OrgUnitStatus Status { get; private set; } = OrgUnitStatus.Active;

    /// <summary>
    /// 同一上级下的次序
    /// </summary>
    public int SortOrder { get; private set; }

    internal static OrgUnit Create(OrgUnitId? parentId, string name, string? code, int sortOrder) =>
        new(OrgUnitId.New(), parentId, name, code, sortOrder);

    internal void Rename(string name) => Name = name;

    internal void ChangeCode(string? code) => Code = code;

    internal void MoveTo(OrgUnitId parentId, int sortOrder)
    {
        ParentId = parentId;
        SortOrder = sortOrder;
    }

    internal void Reposition(int sortOrder) => SortOrder = sortOrder;

    internal void Deactivate() => Status = OrgUnitStatus.Disabled;

    internal void Activate() => Status = OrgUnitStatus.Active;
}
