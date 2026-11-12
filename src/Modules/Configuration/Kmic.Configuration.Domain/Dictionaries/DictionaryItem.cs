using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典项：业务数据里存的是它的编码，名称可以改，编码创建后不能改
/// </summary>
public sealed class DictionaryItem : Entity<DictionaryItemId>
{
    private DictionaryItem(DictionaryItemId id, string code, string name, string? parentCode, int sortOrder)
        : base(id)
    {
        Code = code;
        Name = name;
        ParentCode = parentCode;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 编码：同一字典类型内唯一，不区分大小写，创建后不能改
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// 显示名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 层级字典里父项的编码；顶层为空
    /// </summary>
    public string? ParentCode { get; private set; }

    /// <summary>
    /// 在同一父项下的次序
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public DictionaryItemStatus Status { get; private set; } = DictionaryItemStatus.Active;

    internal static DictionaryItem Create(string code, string name, string? parentCode, int sortOrder) =>
        new(DictionaryItemId.New(), code, name, parentCode, sortOrder);

    internal void Rename(string name) => Name = name;

    internal void MoveTo(string? parentCode, int sortOrder)
    {
        ParentCode = parentCode;
        SortOrder = sortOrder;
    }

    internal void Reposition(int sortOrder) => SortOrder = sortOrder;

    internal void Deactivate() => Status = DictionaryItemStatus.Disabled;

    internal void Activate() => Status = DictionaryItemStatus.Active;
}
