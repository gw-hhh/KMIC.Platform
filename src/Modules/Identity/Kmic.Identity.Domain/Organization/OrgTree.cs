using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Organization;

/// <summary>组织树聚合，适用于数千个单元以内的组织规模。</summary>
public sealed class OrgTree : AggregateRoot<OrgTreeId>
{
    private const int SortStep = 10;

    private readonly List<OrgUnit> _units = [];

    private OrgTree(OrgTreeId id)
        : base(id)
    {
    }

    /// <summary>
    /// 全部单元，含已停用的
    /// </summary>
    public IReadOnlyList<OrgUnit> Units => _units;

    /// <summary>
    /// 根单元
    /// </summary>
    public OrgUnit Root => _units.Find(u => u.ParentId is null) ?? throw new InvalidOperationException("组织树没有根单元。");

    /// <summary>创建组织树及其根单元。</summary>
    public static Result<OrgTree> Create(string? rootName, string? rootCode = null)
    {
        var name = Name(rootName);
        var code = Code(rootCode);
        if (Result.FirstFailure(name, code) is { IsFailure: true } failed)
        {
            return failed.Error;
        }

        var tree = new OrgTree(OrgTreeId.Default);
        tree._units.Add(OrgUnit.Create(null, name.Value, code.Value, SortStep));
        return tree;
    }

    public OrgUnit? Find(OrgUnitId id) => _units.Find(u => u.Id == id);

    /// <summary>获取指定单元自身及全部下级（含已停用）。</summary>
    public IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id)
    {
        var result = new HashSet<OrgUnitId>();
        if (Find(id) is null)
        {
            return result;
        }

        var childrenByParent = _units.Where(u => u.ParentId is not null).ToLookup(u => u.ParentId!.Value);
        var pending = new Stack<OrgUnitId>([id]);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (result.Add(current))
            {
                foreach (var child in childrenByParent[current])
                {
                    pending.Push(child.Id);
                }
            }
        }

        return result;
    }

    /// <summary>获取指定单元的全部上级，按由近及远排序。</summary>
    public IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id)
    {
        var ancestors = new List<OrgUnitId>();
        var current = Find(id)?.ParentId;
        while (current is { } parentId && ancestors.Count <= _units.Count)
        {
            ancestors.Add(parentId);
            current = Find(parentId)?.ParentId;
        }

        return ancestors;
    }

    /// <summary>在已启用的上级单元下添加下级。</summary>
    public Result<OrgUnit> AddUnit(OrgUnitId parentId, string? name, string? code = null)
    {
        var parsedName = Name(name);
        var parsedCode = Code(code);
        if (Result.FirstFailure(parsedName, parsedCode) is { IsFailure: true } failed)
        {
            return failed.Error;
        }

        var parent = Find(parentId);
        if (parent is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        if (parent.Status != OrgUnitStatus.Active)
        {
            return IdentityErrors.OrgUnitDisabled;
        }

        if (SiblingNameTaken(parentId, parsedName.Value, except: null))
        {
            return IdentityErrors.OrgUnitNameExists;
        }

        var sortOrder = _units.Where(u => u.ParentId == parentId).Select(u => u.SortOrder).DefaultIfEmpty(0).Max() + SortStep;
        var unit = OrgUnit.Create(parentId, parsedName.Value, parsedCode.Value, sortOrder);
        _units.Add(unit);
        return unit;
    }

    public Result Rename(OrgUnitId id, string? name)
    {
        var parsed = Name(name);
        if (parsed.IsFailure)
        {
            return parsed.Error;
        }

        var unit = Find(id);
        if (unit is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        if (unit.ParentId is { } parentId && SiblingNameTaken(parentId, parsed.Value, except: id))
        {
            return IdentityErrors.OrgUnitNameExists;
        }

        unit.Rename(parsed.Value);
        return Result.Success();
    }

    /// <summary>修改组织单元编码；传入空值时清除。</summary>
    public Result ChangeCode(OrgUnitId id, string? code)
    {
        var parsed = Code(code);
        if (parsed.IsFailure)
        {
            return parsed.Error;
        }

        var unit = Find(id);
        if (unit is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        unit.ChangeCode(parsed.Value);
        return Result.Success();
    }

    /// <summary>将组织单元移至另一已启用单元下；根单元不可移动，且不可移至自身或下级。</summary>
    public Result Move(OrgUnitId id, OrgUnitId newParentId)
    {
        var unit = Find(id);
        var newParent = Find(newParentId);
        if (unit is null || newParent is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        if (unit.ParentId is null)
        {
            return IdentityErrors.OrgRootImmutable;
        }

        if (SelfAndDescendants(id).Contains(newParentId))
        {
            return IdentityErrors.OrgUnitCycle;
        }

        if (newParent.Status != OrgUnitStatus.Active)
        {
            return IdentityErrors.OrgUnitDisabled;
        }

        if (unit.ParentId == newParentId)
        {
            return Result.Success();
        }

        if (SiblingNameTaken(newParentId, unit.Name, except: id))
        {
            return IdentityErrors.OrgUnitNameExists;
        }

        unit.MoveTo(newParentId, _units.Where(u => u.ParentId == newParentId).Select(u => u.SortOrder).DefaultIfEmpty(0).Max() + SortStep);
        return Result.Success();
    }

    /// <summary>调整直属下级排序，列表须包含全部直属下级且不可重复。</summary>
    public Result Reorder(OrgUnitId parentId, IReadOnlyList<OrgUnitId> orderedChildren)
    {
        ArgumentNullException.ThrowIfNull(orderedChildren);

        if (Find(parentId) is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        var children = _units.Where(u => u.ParentId == parentId).ToList();
        if (orderedChildren.Count != children.Count || orderedChildren.Distinct().Count() != children.Count || children.Exists(c => !orderedChildren.Contains(c.Id)))
        {
            return IdentityErrors.OrgUnitOrderInvalid;
        }

        for (var i = 0; i < orderedChildren.Count; i++)
        {
            Find(orderedChildren[i])!.Reposition((i + 1) * SortStep);
        }

        return Result.Success();
    }

    /// <summary>停用组织单元；根单元或存在启用下级/所属用户时拒绝。</summary>
    public Result Disable(OrgUnitId id, bool hasEnabledUsers)
    {
        var unit = Find(id);
        if (unit is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        if (unit.ParentId is null)
        {
            return IdentityErrors.OrgRootImmutable;
        }

        if (unit.Status == OrgUnitStatus.Disabled)
        {
            return Result.Success();
        }

        if (hasEnabledUsers || _units.Exists(u => u.ParentId == id && u.Status == OrgUnitStatus.Active))
        {
            return IdentityErrors.OrgUnitInUse;
        }

        unit.Deactivate();
        return Result.Success();
    }

    /// <summary>启用组织单元；上级单元须处于启用状态。</summary>
    public Result Enable(OrgUnitId id)
    {
        var unit = Find(id);
        if (unit is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        if (unit.ParentId is { } parentId && Find(parentId)?.Status != OrgUnitStatus.Active)
        {
            return IdentityErrors.OrgUnitDisabled;
        }

        unit.Activate();
        return Result.Success();
    }

    private bool SiblingNameTaken(OrgUnitId parentId, string name, OrgUnitId? except) =>
        _units.Exists(u => u.ParentId == parentId && u.Id != except && string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));

    private static Result<string> Name(string? input)
    {
        var name = input?.Trim();
        return string.IsNullOrEmpty(name) || name.Length > 100 || name.Any(char.IsControl) ? IdentityErrors.OrgUnitNameInvalid : name;
    }

    private static Result<string?> Code(string? input)
    {
        var code = string.IsNullOrWhiteSpace(input) ? null : input.Trim();
        return code is not null && (code.Length > 50 || code.Any(c => char.IsWhiteSpace(c) || char.IsControl(c))) ? IdentityErrors.OrgUnitNameInvalid.WithDetail("编码最多 50 个字符，不能包含空白或控制字符") : code;
    }
}
