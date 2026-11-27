using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>字典类型聚合，包含其全部字典项</summary>
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

    /// <summary>登记代码中新增的字典类型</summary>
    public static DictionaryType Register(string code, string ownerModule, string displayName, bool isHierarchical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new DictionaryType(DictionaryTypeId.New(), code, ownerModule, displayName, isHierarchical);
    }

    /// <summary>按代码声明同步显示名、层级属性及启用状态，返回是否有变更</summary>
    public bool Synchronize(string displayName, bool isHierarchical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var changed = !string.Equals(DisplayName, displayName, StringComparison.Ordinal) || IsHierarchical != isHierarchical || Status != DictionaryTypeStatus.Active;
        DisplayName = displayName;
        IsHierarchical = isHierarchical;
        Status = DictionaryTypeStatus.Active;
        return changed;
    }

    /// <summary>将代码中已移除的类型标记为已废弃（保留历史数据），返回是否有变更</summary>
    public bool Retire()
    {
        if (Status == DictionaryTypeStatus.Retired)
        {
            return false;
        }

        Status = DictionaryTypeStatus.Retired;
        return true;
    }

    /// <summary>按编码查找字典项（不区分大小写）</summary>
    public DictionaryItem? FindItem(string? code) => code is null ? null : _items.Find(i => string.Equals(i.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>检查字典项是否存在且已启用</summary>
    public bool IsSelectable(string? code) => FindItem(code)?.Status == DictionaryItemStatus.Active;

    /// <summary>添加字典项（非层级字典的 parentCode 须为空）</summary>
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

    /// <summary>修改字典项显示名</summary>
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

    /// <summary>变更父项并排在新同级末尾；未指定新父项时移至顶层</summary>
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

    /// <summary>调整同级字典项顺序（须包含全部同级项且不可重复）</summary>
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

    /// <summary>停用字典项（存在启用子项时拒绝）</summary>
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

    /// <summary>启用字典项（有父项时父项须已启用）</summary>
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

    // 父项为空表示顶层；非空时字典须支持层级，且父项须已启用且非自身或后代
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
