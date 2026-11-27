namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>数据权限等级。</summary>
public enum ScopeLevel
{
    /// <summary>仅本人。</summary>
    Self,

    /// <summary>本人及所属组织。</summary>
    OrgUnit,

    /// <summary>本人、所属组织及所有下级组织。</summary>
    OrgUnitAndBelow,

    /// <summary>全部数据。</summary>
    All,
}
