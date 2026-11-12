using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Organization;

/// <summary>
/// 组织树：整棵树是一个聚合
/// </summary>
/// <remarks>
/// 不成环、同级不重名、停用的单元下没有启用的下级，这些约束跨越多个节点，只有把整棵树放进一个聚合才能在一处保证。
/// 树的并发版本号就是组织树版本号，访问上下文按它缓存树快照。规模按不超过几千个单元设计
/// </remarks>
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

    /// <summary>
    /// 创建组织树和它的根单元
    /// </summary>
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

    /// <summary>
    /// 按 ID 查找单元
    /// </summary>
    public OrgUnit? Find(OrgUnitId id) => _units.Find(u => u.Id == id);

    /// <summary>
    /// 自身及全部下级，含已停用的
    /// </summary>
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

    /// <summary>
    /// 全部上级，由近到远
    /// </summary>
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

    /// <summary>
    /// 在某个启用的单元下添加下级
    /// </summary>
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

    /// <summary>
    /// 改名
    /// </summary>
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

    /// <summary>
    /// 修改编码，传空清除
    /// </summary>
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

    /// <summary>
    /// 移到另一个启用的单元之下；不能移到自己或自己的下级之下，根不能移动
    /// </summary>
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

    /// <summary>
    /// 重排某个上级下的全部下级；传入的必须恰好是现有的全部下级
    /// </summary>
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

    /// <summary>
    /// 停用；下面有启用的下级或启用的用户时拒绝，根不能停用
    /// </summary>
    /// <param name="id">单元</param>
    /// <param name="hasEnabledUsers">是否还有启用的用户归属于它，用户在别的聚合里，由调用方查好传入</param>
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

    /// <summary>
    /// 启用；上级必须是启用的
    /// </summary>
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
        return code is not null && (code.Length > 50 || code.Any(c => char.IsWhiteSpace(c) || char.IsControl(c))) ? IdentityErrors.OrgUnitNameInvalid.WithDetail("编码最长 50 个字符，不能含空白") : code;
    }
}
