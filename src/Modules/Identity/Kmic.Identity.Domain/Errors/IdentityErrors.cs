using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Domain.Errors;

/// <summary>
/// Identity 模块的错误目录
/// </summary>
public static class IdentityErrors
{
    /// <summary>
    /// 用户不存在
    /// </summary>
    public static readonly Error UserNotFound = Error.NotFound("Identity.UserNotFound", "用户不存在");

    /// <summary>
    /// 角色不存在
    /// </summary>
    public static readonly Error RoleNotFound = Error.NotFound("Identity.RoleNotFound", "角色不存在");

    /// <summary>
    /// 组织单元不存在
    /// </summary>
    public static readonly Error OrgUnitNotFound = Error.NotFound("Identity.OrgUnitNotFound", "组织单元不存在");

    /// <summary>
    /// 会话不存在
    /// </summary>
    public static readonly Error SessionNotFound = Error.NotFound("Identity.SessionNotFound", "会话不存在");

    /// <summary>
    /// 账号已被使用
    /// </summary>
    public static readonly Error AccountExists = Error.Conflict("Identity.AccountExists", "账号已存在");

    /// <summary>
    /// 手机号已被其他用户使用
    /// </summary>
    public static readonly Error MobileExists = Error.Conflict("Identity.MobileExists", "手机号已被其他用户使用");

    /// <summary>
    /// 工号已被其他用户使用
    /// </summary>
    public static readonly Error EmployeeNoExists = Error.Conflict("Identity.EmployeeNoExists", "工号已被其他用户使用");

    /// <summary>
    /// 角色名称已被使用
    /// </summary>
    public static readonly Error RoleNameExists = Error.Conflict("Identity.RoleNameExists", "角色名称已存在");

    /// <summary>
    /// 同一上级下已有同名的组织单元
    /// </summary>
    public static readonly Error OrgUnitNameExists = Error.Conflict("Identity.OrgUnitNameExists", "同一上级下已有同名的组织单元");

    /// <summary>
    /// 账号格式不对
    /// </summary>
    public static readonly Error AccountInvalid = Error.Validation("Identity.AccountInvalid", "账号不能为空，最长 100 个字符，不能含空白或控制字符");

    /// <summary>
    /// 姓名格式不对
    /// </summary>
    public static readonly Error DisplayNameInvalid = Error.Validation("Identity.DisplayNameInvalid", "姓名不能为空，最长 100 个字符");

    /// <summary>
    /// 手机号格式不对
    /// </summary>
    public static readonly Error MobileInvalid = Error.Validation("Identity.MobileInvalid", "手机号格式不对");

    /// <summary>
    /// 邮箱格式不对
    /// </summary>
    public static readonly Error EmailInvalid = Error.Validation("Identity.EmailInvalid", "邮箱格式不对");

    /// <summary>
    /// 工号格式不对
    /// </summary>
    public static readonly Error EmployeeNoInvalid = Error.Validation("Identity.EmployeeNoInvalid", "工号最长 50 个字符，不能含空白或控制字符");

    /// <summary>
    /// 系统账号不能被修改、授予或登录
    /// </summary>
    public static readonly Error SystemUserImmutable = Error.BusinessRule("Identity.SystemUserImmutable", "系统账号不能修改");

    /// <summary>
    /// 当前状态不能执行这个操作
    /// </summary>
    public static readonly Error InvalidState = Error.BusinessRule("Identity.InvalidState", "当前状态不能执行这个操作");

    /// <summary>
    /// 角色已停用，不能授予
    /// </summary>
    public static readonly Error RoleDisabled = Error.BusinessRule("Identity.RoleDisabled", "角色已停用");

    /// <summary>
    /// 系统管理员这类内置角色不能停用、不能手工改权限点
    /// </summary>
    public static readonly Error SystemRoleImmutable = Error.BusinessRule("Identity.SystemRoleImmutable", "内置的系统管理员角色不能停用，也不能手工修改权限点");

    /// <summary>
    /// 角色名称格式不对
    /// </summary>
    public static readonly Error RoleNameInvalid = Error.Validation("Identity.RoleNameInvalid", "角色名称不能为空，最长 100 个字符");

    /// <summary>
    /// 授予已存在
    /// </summary>
    public static readonly Error GrantExists = Error.Conflict("Identity.GrantExists", "用户已经拥有这个角色");

    /// <summary>
    /// 用户没有这个角色
    /// </summary>
    public static readonly Error GrantNotFound = Error.NotFound("Identity.GrantNotFound", "用户没有这个角色");

    /// <summary>
    /// 组织单元已停用
    /// </summary>
    public static readonly Error OrgUnitDisabled = Error.BusinessRule("Identity.OrgUnitDisabled", "组织单元已停用");

    /// <summary>
    /// 组织单元有启用的下级或用户，不能停用
    /// </summary>
    public static readonly Error OrgUnitInUse = Error.Conflict("Identity.OrgUnitInUse", "组织单元下还有启用的下级或用户，不能停用");

    /// <summary>
    /// 不能把组织单元移到自己或自己的下级之下
    /// </summary>
    public static readonly Error OrgUnitCycle = Error.BusinessRule("Identity.OrgUnitCycle", "不能把组织单元移到自己或自己的下级之下");

    /// <summary>
    /// 根组织单元不能移动、停用
    /// </summary>
    public static readonly Error OrgRootImmutable = Error.BusinessRule("Identity.OrgRootImmutable", "根组织单元不能移动或停用");

    /// <summary>
    /// 组织树里已经有根单元
    /// </summary>
    public static readonly Error OrgRootExists = Error.Conflict("Identity.OrgRootExists", "组织树已经有根单元");

    /// <summary>
    /// 组织单元名称格式不对
    /// </summary>
    public static readonly Error OrgUnitNameInvalid = Error.Validation("Identity.OrgUnitNameInvalid", "组织单元名称不能为空，最长 100 个字符");

    /// <summary>
    /// 重新排序时给出的组织单元和实际的下级不一致
    /// </summary>
    public static readonly Error OrgUnitOrderInvalid = Error.Validation("Identity.OrgUnitOrderInvalid", "排序必须包含该上级下的全部组织单元，且不能重复");

    /// <summary>
    /// 受保护角色只有受保护角色的持有人能管理
    /// </summary>
    public static readonly Error ProtectedRoleChangeDenied = Error.Forbidden("Identity.ProtectedRoleChangeDenied", "只有拥有受保护角色的人才能管理受保护角色及其持有人的凭据");

    /// <summary>
    /// 任何人不能修改自己的授予
    /// </summary>
    public static readonly Error CannotChangeOwnGrants = Error.Forbidden("Identity.CannotChangeOwnGrants", "不能修改自己的角色授予");

    /// <summary>
    /// 系统里必须至少留一个启用的受保护角色持有人
    /// </summary>
    public static readonly Error LastProtectedHolder = Error.BusinessRule("Identity.LastProtectedHolder", "系统中必须至少保留一个启用的系统管理员");

    /// <summary>
    /// 不能对自己做这个操作
    /// </summary>
    public static readonly Error CannotActOnSelf = Error.Forbidden("Identity.CannotActOnSelf", "不能对自己执行这个操作");

    /// <summary>
    /// 授权管理命令提交前重查操作者没通过
    /// </summary>
    public static readonly Error AuthorizationChanged = Error.Conflict("Identity.AuthorizationChanged", "你的授权刚刚发生了变化，请刷新后重试")
        .WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 权限点不存在或已退役
    /// </summary>
    public static readonly Error PermissionUnknown = Error.Validation("Identity.PermissionUnknown", "权限点不存在或已退役");

    /// <summary>
    /// 范围对象不存在
    /// </summary>
    public static readonly Error ScopeObjectUnknown = Error.Validation("Identity.ScopeObjectUnknown", "数据范围对象不存在");

    /// <summary>
    /// 账号或密码不对，也可能是账号被禁用；对外不区分
    /// </summary>
    public static readonly Error InvalidCredentials = Error.Unauthenticated("Identity.InvalidCredentials", "账号或密码不对");

    /// <summary>
    /// 验证码不对、已过期或已用完次数
    /// </summary>
    public static readonly Error InvalidSmsCode = Error.Unauthenticated("Identity.InvalidSmsCode", "验证码不对或已过期");

    /// <summary>
    /// 刷新令牌无效
    /// </summary>
    public static readonly Error InvalidRefreshToken = Error.Unauthenticated("Identity.InvalidRefreshToken", "登录已失效，请重新登录");

    /// <summary>
    /// 登录尝试过多，要等一段时间
    /// </summary>
    public static readonly Error LoginThrottled = Error.TooManyRequests("Identity.LoginThrottled", "登录尝试过多，请稍后再试或联系管理员解锁", TimeSpan.FromMinutes(1));

    /// <summary>
    /// 验证码发送过于频繁
    /// </summary>
    public static readonly Error SmsCodeThrottled = Error.TooManyRequests("Identity.SmsCodeThrottled", "验证码发送过于频繁，请稍后再试", TimeSpan.FromMinutes(1));

    /// <summary>
    /// 没有可用的角色，登录不了
    /// </summary>
    public static readonly Error NoRoleAvailable = Error.Forbidden("Identity.NoRoleAvailable", "你没有可用的角色，请联系管理员");

    /// <summary>
    /// 切换的角色不属于当前用户或已停用
    /// </summary>
    public static readonly Error RoleNotSwitchable = Error.Forbidden("Identity.RoleNotSwitchable", "不能切换到这个角色");

    /// <summary>
    /// 旧密码不对
    /// </summary>
    public static readonly Error OldPasswordWrong = Error.Validation("Identity.OldPasswordWrong", "旧密码不对");

    /// <summary>
    /// 新密码不符合密码规则
    /// </summary>
    public static readonly Error PasswordTooWeak = Error.Validation("Identity.PasswordTooWeak", "密码不符合要求：长度要在规定范围内，不能是常见的弱密码，也不能与账号或手机号相同");

    /// <summary>
    /// 必须先修改密码才能继续
    /// </summary>
    public static readonly Error PasswordChangeRequired = Error.Forbidden("Identity.PasswordChangeRequired", "请先修改密码");

    /// <summary>
    /// 模拟身份的目标不合法
    /// </summary>
    public static readonly Error ImpersonationDenied = Error.Forbidden("Identity.ImpersonationDenied", "不能模拟这个用户：不能模拟自己，也不能模拟系统管理员");

    /// <summary>
    /// 当前会话不是模拟会话
    /// </summary>
    public static readonly Error NotImpersonating = Error.BusinessRule("Identity.NotImpersonating", "当前不是模拟身份");

    /// <summary>
    /// 初始化只能在系统里没有任何用户时执行
    /// </summary>
    public static readonly Error AlreadyBootstrapped = Error.Conflict("Identity.AlreadyBootstrapped", "系统里已经有用户，不能再初始化");

    /// <summary>
    /// 应急重置只能对持有受保护角色的账号执行
    /// </summary>
    public static readonly Error NotProtectedHolder = Error.BusinessRule("Identity.NotProtectedHolder", "这个账号不是系统管理员，不能用应急命令重置");
}
