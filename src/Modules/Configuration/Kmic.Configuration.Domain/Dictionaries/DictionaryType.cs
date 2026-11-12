using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典类型和它的全部字典项
/// </summary>
/// <remarks>
/// 字典类型由代码声明，启动时同步进来；字典项由管理员维护。一个类型一个聚合：编码唯一、父项存在、不成环、
/// 停用时没有启用的子项这些跨项的规则，在聚合里一处保证，字典项的变化都递增类型的版本
/// </remarks>
public sealed partial class DictionaryType : AggregateRoot<DictionaryTypeId>
{
    private const int SortStep = 10;
    private const int MaxNameLength = 200;

    private readonly List<DictionaryItem> _items = [];

    private DictionaryType(DictionaryTypeId id, string code, string ownerModule, string displayName, bool isHierarchical)
        : base(id)
    {
        Code = code;
        OwnerModule = ownerModule;
        DisplayName = displayName;
        IsHierarchical = isHierarchical;
    }

    /// <summary>
    /// 类型编码，以声明它的模块名开头，如 Sample.Industry
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// 声明它的模块
    /// </summary>
    public string OwnerModule { get; private set; }

    /// <summary>
    /// 显示名，来自代码声明，每次同步更新
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// 是否层级字典
    /// </summary>
    public bool IsHierarchical { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public DictionaryTypeStatus Status { get; private set; } = DictionaryTypeStatus.Active;

    /// <summary>
    /// 全部字典项，含已停用的
    /// </summary>
    public IReadOnlyList<DictionaryItem> Items => _items;

    /// <summary>
    /// 登记一个新的字典类型：代码里新增了声明时由启动任务调用
    /// </summary>
    public static DictionaryType Register(string code, string ownerModule, string displayName, bool isHierarchical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new DictionaryType(DictionaryTypeId.New(), code, ownerModule, displayName, isHierarchical);
    }

    /// <summary>
    /// 按代码里的声明更新显示名与层级属性，退役的类型重新出现时回到使用中；返回有没有变化
    /// </summary>
    public bool Synchronize(string displayName, bool isHierarchical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var changed = !string.Equals(DisplayName, displayName, StringComparison.Ordinal) || IsHierarchical != isHierarchical || Status != DictionaryTypeStatus.Active;
        DisplayName = displayName;
        IsHierarchical = isHierarchical;
        Status = DictionaryTypeStatus.Active;
        return changed;
    }

    /// <summary>
    /// 代码里已经没有这个类型：标为退役，数据保留；返回有没有变化
    /// </summary>
    public bool Retire()
    {
        if (Status == DictionaryTypeStatus.Retired)
        {
            return false;
        }

        Status = DictionaryTypeStatus.Retired;
        return true;
    }

    /// <summary>
    /// 按编码找字典项，不区分大小写
    /// </summary>
    public DictionaryItem? FindItem(string? code) => code is null ? null : _items.Find(i => string.Equals(i.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 这个编码现在能不能选用：存在而且启用
    /// </summary>
    public bool IsSelectable(string? code) => FindItem(code)?.Status == DictionaryItemStatus.Active;

    /// <summary>
    /// 添加字典项
    /// </summary>
    /// <param name="code">编码，字母、数字、点、短横线、下划线，1 到 50 位</param>
    /// <param name="name">显示名</param>
    /// <param name="parentCode">父项编码；不分层的字典必须为空</param>
    /// <param name="sortOrder">次序；不给就排在同级最后</param>
    /// <param name="maxItems">这个类型最多多少项</param>
    public Result<DictionaryItem> AddItem(string? code, string? name, string? parentCode, int? sortOrder, int maxItems)
    {
        var active = EnsureActive();
        if (active.IsFailure)
        {
            return active.Error;
        }

        var cleanCode = code?.Trim();
        if (string.IsNullOrEmpty(cleanCode) || !CodePattern().IsMatch(cleanCode))
        {
            return ConfigurationErrors.DictionaryItemCodeInvalid;
        }

        var cleanName = CleanName(name);
        if (cleanName is null)
        {
            return ConfigurationErrors.DictionaryItemNameInvalid;
        }

        if (FindItem(cleanCode) is not null)
        {
            return ConfigurationErrors.DictionaryItemCodeExists;
        }

        if (_items.Count >= maxItems)
        {
            return ConfigurationErrors.DictionaryTooManyItems;
        }

        var parent = CheckParent(parentCode, self: null);
        if (parent.IsFailure)
        {
            return parent.Error;
        }

        var item = DictionaryItem.Create(cleanCode, cleanName, parent.Value?.Code, sortOrder ?? NextSortOrder(parent.Value?.Code));
        _items.Add(item);
        return item;
    }

    /// <summary>
    /// 改显示名
    /// </summary>
    public Result RenameItem(string? code, string? name)
    {
        var found = Find(code);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var cleanName = CleanName(name);
        if (cleanName is null)
        {
            return ConfigurationErrors.DictionaryItemNameInvalid;
        }

        found.Value.Rename(cleanName);
        return Result.Success();
    }

    /// <summary>
    /// 层级字典里改父项，排在新父项下的最后；不给新父项就移到顶层
    /// </summary>
    public Result MoveItem(string? code, string? newParentCode)
    {
        var found = Find(code);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var item = found.Value;
        var parent = CheckParent(newParentCode, item);
        if (parent.IsFailure)
        {
            return parent.Error;
        }

        if (string.Equals(item.ParentCode, parent.Value?.Code, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success();
        }

        item.MoveTo(parent.Value?.Code, NextSortOrder(parent.Value?.Code));
        return Result.Success();
    }

    /// <summary>
    /// 调整同一父项下全部字典项的次序：必须给全，不能多、不能重复，排序重排为 10、20、30……
    /// </summary>
    public Result ReorderItems(string? parentCode, IReadOnlyList<string> orderedCodes)
    {
        ArgumentNullException.ThrowIfNull(orderedCodes);

        var active = EnsureActive();
        if (active.IsFailure)
        {
            return active;
        }

        var siblings = _items.Where(i => string.Equals(i.ParentCode, parentCode, StringComparison.OrdinalIgnoreCase)).ToList();
        var ordered = orderedCodes.Select(c => FindItem(c)).ToList();
        if (ordered.Count != siblings.Count || ordered.Exists(i => i is null) || ordered.Distinct().Count() != ordered.Count || ordered.Exists(i => !siblings.Contains(i!)))
        {
            return ConfigurationErrors.DictionaryOrderInvalid;
        }

        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index]!.Reposition((index + 1) * SortStep);
        }

        return Result.Success();
    }

    /// <summary>
    /// 停用：下面有启用的子项时拒绝
    /// </summary>
    public Result DisableItem(string? code)
    {
        var found = Find(code);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var item = found.Value;
        if (item.Status == DictionaryItemStatus.Disabled)
        {
            return ConfigurationErrors.InvalidState;
        }

        if (_items.Exists(i => i.Status == DictionaryItemStatus.Active && string.Equals(i.ParentCode, item.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return ConfigurationErrors.DictionaryItemHasEnabledChildren;
        }

        item.Deactivate();
        return Result.Success();
    }

    /// <summary>
    /// 启用：父项必须是启用的
    /// </summary>
    public Result EnableItem(string? code)
    {
        var found = Find(code);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var item = found.Value;
        if (item.Status == DictionaryItemStatus.Active)
        {
            return ConfigurationErrors.InvalidState;
        }

        if (item.ParentCode is not null && FindItem(item.ParentCode)?.Status != DictionaryItemStatus.Active)
        {
            return ConfigurationErrors.DictionaryParentInvalid;
        }

        item.Activate();
        return Result.Success();
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,50}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 200)]
    private static partial Regex CodePattern();

    private static string? CleanName(string? name)
    {
        var clean = name?.Trim();
        return string.IsNullOrEmpty(clean) || clean.Length > MaxNameLength ? null : clean;
    }

    private Result EnsureActive() => Status == DictionaryTypeStatus.Retired ? ConfigurationErrors.DictionaryTypeRetired : Result.Success();

    private Result<DictionaryItem> Find(string? code)
    {
        var active = EnsureActive();
        if (active.IsFailure)
        {
            return active.Error;
        }

        return FindItem(code) is { } item ? item : ConfigurationErrors.DictionaryItemNotFound;
    }

    // 父项为空表示顶层；不空时字典必须分层，父项存在且启用，而且不能是自己或自己的后代
    private Result<DictionaryItem?> CheckParent(string? parentCode, DictionaryItem? self)
    {
        if (string.IsNullOrWhiteSpace(parentCode))
        {
            return (DictionaryItem?)null;
        }

        var parent = IsHierarchical ? FindItem(parentCode) : null;
        if (parent is null || parent.Status != DictionaryItemStatus.Active || (self is not null && IsSelfOrDescendant(self, parent)))
        {
            return ConfigurationErrors.DictionaryParentInvalid;
        }

        return parent;
    }

    private bool IsSelfOrDescendant(DictionaryItem root, DictionaryItem candidate)
    {
        for (var current = candidate; current is not null; current = FindItem(current.ParentCode))
        {
            if (ReferenceEquals(current, root))
            {
                return true;
            }
        }

        return false;
    }

    private int NextSortOrder(string? parentCode) =>
        _items.Where(i => string.Equals(i.ParentCode, parentCode, StringComparison.OrdinalIgnoreCase)).Select(i => i.SortOrder).DefaultIfEmpty(0).Max() + SortStep;
}
