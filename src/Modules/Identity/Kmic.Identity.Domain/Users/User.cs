using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Domain.Users;

/// <summary>用户聚合（可登录的自然人账号或内置系统账号），仅禁用不删除。</summary>
public sealed class User : AggregateRoot<UserId>
{
    private static readonly StateMachine<User, UserStatus, UserAction> _machine =
        StateMachine.Define<User, UserStatus, UserAction>(u => u.Status, (u, v) => u.Status = v)
            .Initial(UserStatus.Active)
            .OnInvalid(IdentityErrors.InvalidState)
            .Permit(UserStatus.Active, UserAction.Disable, UserStatus.Disabled)
            .Permit(UserStatus.Disabled, UserAction.Enable, UserStatus.Active)
            .Build();

    private readonly List<UserRoleGrant> _grants = [];

    private User(UserId id, string account, string displayName)
        : base(id)
    {
        Account = account;
        DisplayName = displayName;
    }

    /// <summary>
    /// 登录名，唯一，不区分大小写
    /// </summary>
    public string Account { get; private set; }

    /// <summary>
    /// 姓名，别的模块显示人名时经引用查询取
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// 英文名
    /// </summary>
    public string? EnglishName { get; private set; }

    /// <summary>
    /// 工号，填写时唯一
    /// </summary>
    public string? EmployeeNo { get; private set; }

    /// <summary>
    /// 手机号，国际格式，填写时唯一
    /// </summary>
    public string? Mobile { get; private set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// 所属组织单元
    /// </summary>
    /// <remarks>
    /// 授予没设锚点时，作为“本组织”的起点
    /// </remarks>
    public OrgUnitId? OrgUnitId { get; private set; }

    /// <summary>
    /// 种类
    /// </summary>
    public UserKind Kind { get; private set; } = UserKind.Normal;

    /// <summary>
    /// 状态
    /// </summary>
    public UserStatus Status { get; private set; } = null!;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string? PasswordHash { get; private set; }

    /// <summary>
    /// 登录后是否必须先修改密码
    /// </summary>
    public bool PasswordChangeRequired { get; private set; }

    /// <summary>
    /// 密码最近一次变化的时间
    /// </summary>
    public DateTimeOffset? PasswordChangedAt { get; private set; }

    /// <summary>
    /// 上次使用的角色，下次登录时优先选它
    /// </summary>
    public RoleId? LastRoleId { get; private set; }

    /// <summary>
    /// 角色授予
    /// </summary>
    public IReadOnlyList<UserRoleGrant> Grants => _grants;

    /// <summary>
    /// 能否登录：普通用户且启用
    /// </summary>
    public bool CanLogin => Kind == UserKind.Normal && Status == UserStatus.Active;

    /// <summary>创建用户（暂不设置密码、所属组织和角色）。</summary>
    public static Result<User> Create(string? account, string? displayName, string? englishName, string? employeeNo, string? mobile, string? email, string? reason = null)
    {
        var fields = ReadFields(account, displayName, englishName, employeeNo, mobile, email);
        if (fields.IsFailure)
        {
            return fields.Error;
        }

        var (a, d, e, n, m, mail) = fields.Value;
        var user = new User(UserId.New(), a, d) { EnglishName = e, EmployeeNo = n, Mobile = m, Email = mail };
        _machine.Start(user, reason);
        return user;
    }

    public Result UpdateProfile(string? account, string? displayName, string? englishName, string? employeeNo, string? mobile, string? email)
    {
        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        var fields = ReadFields(account, displayName, englishName, employeeNo, mobile, email);
        if (fields.IsFailure)
        {
            return fields.Error;
        }

        (Account, DisplayName, EnglishName, EmployeeNo, Mobile, Email) = fields.Value;
        return Result.Success();
    }

    /// <summary>检查传入账号或手机号规范化后是否发生变化。</summary>
    public bool WouldChangeCredentials(string? account, string? mobile)
    {
        var newAccount = UserFields.Account(account);
        var newMobile = UserFields.Mobile(mobile);
        return (newAccount.IsSuccess && !string.Equals(newAccount.Value, Account, StringComparison.Ordinal))
            || (newMobile.IsSuccess && !string.Equals(newMobile.Value, Mobile, StringComparison.Ordinal));
    }

    public Result AssignOrgUnit(OrgUnitId? orgUnitId)
    {
        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        OrgUnitId = orgUnitId;
        return Result.Success();
    }

    /// <summary>禁用用户并发布禁用事件，由事件处理器在同一事务内作废其全部会话。</summary>
    public Result Disable(string? reason = null)
    {
        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        var fired = _machine.Fire(this, UserAction.Disable, reason);
        if (fired.IsSuccess)
        {
            Raise(new UserDisabled(Id));
        }

        return fired;
    }

    public Result Enable(string? reason = null)
    {
        var mutable = EnsureMutable();
        return mutable.IsFailure ? mutable : _machine.Fire(this, UserAction.Enable, reason);
    }

    /// <summary>设置密码哈希并发布密码变更事件。</summary>
    public Result SetPassword(string passwordHash, bool mustChange, bool byAdministrator, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        PasswordHash = passwordHash;
        PasswordChangeRequired = mustChange;
        PasswordChangedAt = now;
        Raise(new PasswordChanged(Id, byAdministrator));
        return Result.Success();
    }

    /// <summary>升级密码哈希参数，保留原改密时间且不发布密码变更事件。</summary>
    public void RehashPassword(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public void RecordRoleUsed(RoleId roleId) => LastRoleId = roleId;

    public bool HasRole(RoleId roleId) => _grants.Exists(g => g.RoleId == roleId);

    public Result<UserRoleGrant> Grant(RoleId roleId, UserId grantedBy, DateTimeOffset now, IEnumerable<OrgUnitId> anchors)
    {
        ArgumentNullException.ThrowIfNull(anchors);

        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable.Error;
        }

        if (HasRole(roleId))
        {
            return IdentityErrors.GrantExists;
        }

        var grant = UserRoleGrant.Create(roleId, grantedBy, now, anchors);
        _grants.Add(grant);
        return grant;
    }

    /// <summary>收回角色；若收回的是最近使用角色则一并清空。</summary>
    public Result Revoke(RoleId roleId)
    {
        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        var grant = _grants.Find(g => g.RoleId == roleId);
        if (grant is null)
        {
            return IdentityErrors.GrantNotFound;
        }

        _grants.Remove(grant);
        if (LastRoleId == roleId)
        {
            LastRoleId = null;
        }

        return Result.Success();
    }

    /// <summary>整体替换指定角色授予的组织锚点。</summary>
    public Result SetAnchors(RoleId roleId, IEnumerable<OrgUnitId> anchors)
    {
        ArgumentNullException.ThrowIfNull(anchors);

        var mutable = EnsureMutable();
        if (mutable.IsFailure)
        {
            return mutable;
        }

        var grant = _grants.Find(g => g.RoleId == roleId);
        if (grant is null)
        {
            return IdentityErrors.GrantNotFound;
        }

        grant.SetAnchors(anchors);
        return Result.Success();
    }

    private static Result<(string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email)> ReadFields(
        string? account, string? displayName, string? englishName, string? employeeNo, string? mobile, string? email)
    {
        var a = UserFields.Account(account);
        var d = UserFields.DisplayName(displayName);
        var e = UserFields.EnglishName(englishName);
        var n = UserFields.EmployeeNo(employeeNo);
        var m = UserFields.Mobile(mobile);
        var mail = UserFields.Email(email);
        var failed = Result.FirstFailure(a, d, e, n, m, mail);
        return failed.IsFailure ? failed.Error : (a.Value, d.Value, e.Value, n.Value, m.Value, mail.Value);
    }

    private Result EnsureMutable() => Kind == UserKind.System ? IdentityErrors.SystemUserImmutable : Result.Success();
}
