using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Roles;

/// <summary>角色聚合，包含权限点与数据范围配置；内置系统管理员不持久化权限点行，加载时授予全部有效权限。</summary>
public sealed class Role : AggregateRoot<RoleId>
{
    private static readonly StateMachine<Role, RoleStatus, RoleAction> _machine =
        StateMachine.Define<Role, RoleStatus, RoleAction>(r => r.Status, (r, v) => r.Status = v)
            .Initial(RoleStatus.Active)
            .OnInvalid(IdentityErrors.InvalidState)
            .Permit(RoleStatus.Active, RoleAction.Disable, RoleStatus.Disabled)
            .Permit(RoleStatus.Disabled, RoleAction.Enable, RoleStatus.Active)
            .Build();

    private readonly List<RolePermissionEntry> _permissions = [];
    private readonly List<RoleDataScope> _dataScopes = [];

    private Role(RoleId id, string name, string? description, int sortOrder, RoleScopeLevel defaultScopeLevel)
        : base(id)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        DefaultScopeLevel = defaultScopeLevel;
    }

    /// <summary>
    /// 名称，唯一且不区分大小写
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public RoleStatus Status { get; private set; } = null!;

    /// <summary>
    /// 是否受保护
    /// </summary>
    public bool IsProtected { get; private set; }

    /// <summary>
    /// 是否拥有全部权限点；为真时不存权限点行，新增的权限点自动包含在内
    /// </summary>
    public bool GrantsAllPermissions { get; private set; }

    /// <summary>
    /// 默认的数据范围等级，没有单独设置的范围对象都按它
    /// </summary>
    public RoleScopeLevel DefaultScopeLevel { get; private set; }

    /// <summary>
    /// 登录时角色下拉里的次序
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 全权限角色最近一次同步的权限点目录指纹
    /// </summary>
    public string? CatalogFingerprint { get; private set; }

    /// <summary>
    /// 拥有的权限点
    /// </summary>
    public IReadOnlyList<RolePermissionEntry> Permissions => _permissions;

    /// <summary>
    /// 按范围对象单独设置的数据范围等级
    /// </summary>
    public IReadOnlyList<RoleDataScope> DataScopes => _dataScopes;

    /// <summary>
    /// 是否内置角色：拥有全部权限点，权限点、数据范围不能手工改，也不能停用
    /// </summary>
    public bool IsBuiltIn => GrantsAllPermissions;

    /// <summary>创建普通角色（初始无权限点）。</summary>
    public static Result<Role> Create(string? name, string? description, int sortOrder, RoleScopeLevel defaultScopeLevel, string? reason = null)
    {
        var fields = ReadFields(name, description);
        if (fields.IsFailure)
        {
            return fields.Error;
        }

        var role = new Role(RoleId.New(), fields.Value.Name, fields.Value.Description, sortOrder, defaultScopeLevel);
        _machine.Start(role, reason);
        return role;
    }

    /// <summary>创建内置系统管理员角色（受保护、全权限、全部数据范围）。</summary>
    public static Role CreateSystemAdministrator(string name = "系统管理员")
    {
        var role = new Role(RoleId.New(), name, "拥有全部权限点，管理此角色时须使用受保护角色", 0, RoleScopeLevel.All)
        {
            IsProtected = true,
            GrantsAllPermissions = true,
        };
        _machine.Start(role, "内置角色");
        return role;
    }

    public Result Rename(string? name)
    {
        var parsed = ReadFields(name, Description);
        if (parsed.IsFailure)
        {
            return parsed.Error;
        }

        Name = parsed.Value.Name;
        return Result.Success();
    }

    public Result Describe(string? description)
    {
        var parsed = ReadFields(Name, description);
        if (parsed.IsFailure)
        {
            return parsed.Error;
        }

        Description = parsed.Value.Description;
        return Result.Success();
    }

    public void Reorder(int sortOrder) => SortOrder = sortOrder;

    /// <summary>全量替换角色权限点。</summary>
    public Result SetPermissions(IEnumerable<string> permissionCodes)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);

        if (GrantsAllPermissions)
        {
            return IdentityErrors.SystemRoleImmutable;
        }

        var wanted = permissionCodes.Distinct(StringComparer.Ordinal).ToList();
        _permissions.RemoveAll(p => !wanted.Contains(p.PermissionCode, StringComparer.Ordinal));
        foreach (var code in wanted.Where(c => _permissions.TrueForAll(p => !string.Equals(p.PermissionCode, c, StringComparison.Ordinal))))
        {
            _permissions.Add(RolePermissionEntry.Create(code));
        }

        return Result.Success();
    }

    /// <summary>按映射表替换权限编码，新编码已存在时移除旧项；返回是否发生变更。</summary>
    public bool RenamePermissions(IReadOnlyDictionary<string, string> renames)
    {
        ArgumentNullException.ThrowIfNull(renames);

        var changed = false;
        foreach (var entry in _permissions.Where(p => renames.ContainsKey(p.PermissionCode)).ToList())
        {
            var newCode = renames[entry.PermissionCode];
            if (_permissions.Exists(p => string.Equals(p.PermissionCode, newCode, StringComparison.Ordinal)))
            {
                _permissions.Remove(entry);
            }
            else
            {
                entry.Rename(newCode);
            }

            changed = true;
        }

        return changed;
    }

    /// <summary>移除指定权限配置，返回是否发生变更。</summary>
    public bool RemovePermissions(IReadOnlyCollection<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        return _permissions.RemoveAll(p => codes.Contains(p.PermissionCode, StringComparer.Ordinal)) > 0;
    }

    /// <summary>移除指定数据范围覆盖配置，返回是否发生变更。</summary>
    public bool RemoveDataScopeOverrides(IReadOnlyCollection<string> scopeObjects)
    {
        ArgumentNullException.ThrowIfNull(scopeObjects);

        return _dataScopes.RemoveAll(s => scopeObjects.Contains(s.ScopeObject, StringComparer.Ordinal)) > 0;
    }

    /// <summary>全量替换默认数据范围及各范围对象的覆盖配置。</summary>
    public Result SetDataScopes(RoleScopeLevel defaultLevel, IReadOnlyDictionary<string, RoleScopeLevel> overrides)
    {
        ArgumentNullException.ThrowIfNull(overrides);

        if (GrantsAllPermissions)
        {
            return IdentityErrors.SystemRoleImmutable;
        }

        DefaultScopeLevel = defaultLevel;
        _dataScopes.RemoveAll(s => !overrides.ContainsKey(s.ScopeObject));
        foreach (var (scopeObject, level) in overrides)
        {
            if (_dataScopes.Find(s => string.Equals(s.ScopeObject, scopeObject, StringComparison.Ordinal)) is { } existing)
            {
                existing.ChangeLevel(level);
            }
            else
            {
                _dataScopes.Add(RoleDataScope.Create(scopeObject, level));
            }
        }

        return Result.Success();
    }

    /// <summary>停用角色；内置角色不可停用。</summary>
    public Result Disable(string? reason = null) =>
        GrantsAllPermissions ? IdentityErrors.SystemRoleImmutable : _machine.Fire(this, RoleAction.Disable, reason);

    public Result Enable(string? reason = null) => _machine.Fire(this, RoleAction.Enable, reason);

    /// <summary>同步全权限角色的目录指纹（保存时递增并发版本以使缓存失效）。</summary>
    public void SyncCatalog(string catalogHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogHash);

        if (GrantsAllPermissions)
        {
            CatalogFingerprint = catalogHash;
        }
    }

    private static Result<(string Name, string? Description)> ReadFields(string? name, string? description)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100 || trimmed.Any(char.IsControl))
        {
            return IdentityErrors.RoleNameInvalid;
        }

        var note = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return note is { Length: > 500 } ? IdentityErrors.RoleNameInvalid.WithDetail("说明最长 500 个字符") : (trimmed, note);
    }
}
