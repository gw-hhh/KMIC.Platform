using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户：能登录系统的人，以及代表“系统”的固定账号
/// </summary>
/// <remarks>
/// 只带身份与通用展示字段，职称、行业这类业务属性由使用它们的业务模块自己保存。
/// 用户不删除，只禁用：业务数据里到处引用着负责人、创建人，历史上的人必须还能解析出来
/// </remarks>
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
    /// 登录名，唯一且不区分大小写；迁移来的用户沿用旧系统的手机号
    /// </summary>
    public string Account { get; private set; }

    /// <summary>
    /// 姓名，别的模块显示人名时经引用查询取
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// 英文名，合同、报告上要印
    /// </summary>
    public string? EnglishName { get; private set; }

    /// <summary>
    /// 工号，填写时唯一
    /// </summary>
    public string? EmployeeNo { get; private set; }

    /// <summary>
    /// 手机号，规范化为国际格式，填写时唯一；短信登录需要它
    /// </summary>
    public string? Mobile { get; private set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// 所属组织单元，数据范围的“本组织”从它算起；为空时只剩“本人”的数据
    /// </summary>
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
    /// 密码哈希；为空表示还没有设置过密码（迁移来的员工靠短信登录）
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

    /// <summary>
    /// 创建用户：没有密码、没有所属组织，也没有角色，这些由各自的命令设置
    /// </summary>
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

    /// <summary>
    /// 修改资料
    /// </summary>
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

    /// <summary>
    /// 这次修改会不会改动登录凭据（账号、手机号）
    /// </summary>
    /// <remarks>
    /// 输入本身不合法时当作没有改动：随后的修改会因为同一个原因失败，不必在这里重复报错
    /// </remarks>
    public bool WouldChangeCredentials(string? account, string? mobile)
    {
        var newAccount = UserFields.Account(account);
        var newMobile = UserFields.Mobile(mobile);
        return (newAccount.IsSuccess && !string.Equals(newAccount.Value, Account, StringComparison.Ordinal))
            || (newMobile.IsSuccess && !string.Equals(newMobile.Value, Mobile, StringComparison.Ordinal));
    }

    /// <summary>
    /// 设置所属组织；组织是否存在、是否启用由调用方先确认
    /// </summary>
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

    /// <summary>
    /// 禁用：他的全部会话随之作废
    /// </summary>
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

    /// <summary>
    /// 启用
    /// </summary>
    public Result Enable(string? reason = null)
    {
        var mutable = EnsureMutable();
        return mutable.IsFailure ? mutable : _machine.Fire(this, UserAction.Enable, reason);
    }

    /// <summary>
    /// 设置密码哈希
    /// </summary>
    /// <param name="passwordHash">哈希器的输出</param>
    /// <param name="mustChange">登录后是否必须再改一次，管理员重置时为真</param>
    /// <param name="byAdministrator">是否由管理员重置，决定作废哪些会话</param>
    /// <param name="now">当前时间</param>
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

    /// <summary>
    /// 哈希器要求升级哈希参数时，原地换成新哈希，不算一次密码变化
    /// </summary>
    public void RehashPassword(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// 记下上次使用的角色
    /// </summary>
    public void RecordRoleUsed(RoleId roleId) => LastRoleId = roleId;

    /// <summary>
    /// 是否拥有某个角色
    /// </summary>
    public bool HasRole(RoleId roleId) => _grants.Exists(g => g.RoleId == roleId);

    /// <summary>
    /// 授予角色；角色是否存在、是否启用、锚点是否启用由调用方先确认
    /// </summary>
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

    /// <summary>
    /// 收回角色
    /// </summary>
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

    /// <summary>
    /// 整体替换某个授予上的锚点
    /// </summary>
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
