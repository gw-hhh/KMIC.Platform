using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Domain.Errors;

public static class IdentityErrors
{
    public static readonly Error UserNotFound = Error.NotFound("Identity.UserNotFound", "用户不存在");

    public static readonly Error RoleNotFound = Error.NotFound("Identity.RoleNotFound", "角色不存在");

    public static readonly Error OrgUnitNotFound = Error.NotFound("Identity.OrgUnitNotFound", "组织单元不存在");

    public static readonly Error SessionNotFound = Error.NotFound("Identity.SessionNotFound", "会话不存在");

    public static readonly Error AccountExists = Error.Conflict("Identity.AccountExists", "账号已存在");

    public static readonly Error MobileExists = Error.Conflict("Identity.MobileExists", "手机号已被其他用户使用");

    public static readonly Error EmployeeNoExists = Error.Conflict("Identity.EmployeeNoExists", "工号已被其他用户使用");

    public static readonly Error RoleNameExists = Error.Conflict("Identity.RoleNameExists", "角色名称已存在");

    public static readonly Error OrgUnitNameExists = Error.Conflict("Identity.OrgUnitNameExists", "同一上级下已有同名的组织单元");

    public static readonly Error AccountInvalid = Error.Validation("Identity.AccountInvalid", "账号不能为空，最长 100 个字符，不能含空白或控制字符");

    public static readonly Error DisplayNameInvalid = Error.Validation("Identity.DisplayNameInvalid", "姓名不能为空；姓名和英文名最多 100 个字符，不能含控制字符");

    public static readonly Error MobileInvalid = Error.Validation("Identity.MobileInvalid", "手机号格式无效");

    public static readonly Error EmailInvalid = Error.Validation("Identity.EmailInvalid", "邮箱格式无效");

    public static readonly Error EmployeeNoInvalid = Error.Validation("Identity.EmployeeNoInvalid", "工号最长 50 个字符，不能含空白或控制字符");

    /// <summary>系统账号不可修改、不可登录，也不能向其授予角色。</summary>
    public static readonly Error SystemUserImmutable = Error.BusinessRule("Identity.SystemUserImmutable", "系统账号不能修改");

    public static readonly Error InvalidState = Error.BusinessRule("Identity.InvalidState", "当前状态不允许此操作");

    /// <summary>角色已停用，不能授予。</summary>
    public static readonly Error RoleDisabled = Error.BusinessRule("Identity.RoleDisabled", "角色已停用");

    public static readonly Error SystemRoleImmutable = Error.BusinessRule("Identity.SystemRoleImmutable", "内置系统管理员角色不能停用，也不能修改权限或数据范围");

    public static readonly Error RoleNameInvalid = Error.Validation("Identity.RoleNameInvalid", "角色名称不能为空，最多 100 个字符，不能含控制字符");

    public static readonly Error GrantExists = Error.Conflict("Identity.GrantExists", "用户已拥有该角色");

    public static readonly Error GrantNotFound = Error.NotFound("Identity.GrantNotFound", "用户未被授予该角色");

    public static readonly Error OrgUnitDisabled = Error.BusinessRule("Identity.OrgUnitDisabled", "组织单元已停用");

    public static readonly Error OrgUnitInUse = Error.Conflict("Identity.OrgUnitInUse", "组织单元下还有启用的下级或用户，不能停用");

    public static readonly Error OrgUnitCycle = Error.BusinessRule("Identity.OrgUnitCycle", "不能将组织单元移至自身或其下级下");

    public static readonly Error OrgRootImmutable = Error.BusinessRule("Identity.OrgRootImmutable", "根组织单元不能移动或停用");

    public static readonly Error OrgRootExists = Error.Conflict("Identity.OrgRootExists", "组织树已经有根单元");

    public static readonly Error OrgUnitNameInvalid = Error.Validation("Identity.OrgUnitNameInvalid", "组织单元名称不能为空，最多 100 个字符，不能含控制字符");

    public static readonly Error OrgUnitOrderInvalid = Error.Validation("Identity.OrgUnitOrderInvalid", "排序必须包含该上级下的全部组织单元，且不能重复");

    public static readonly Error ProtectedRoleChangeDenied = Error.Forbidden("Identity.ProtectedRoleChangeDenied", "当前角色须为受保护角色，才能执行此操作");

    public static readonly Error CannotChangeOwnGrants = Error.Forbidden("Identity.CannotChangeOwnGrants", "不能修改自己的角色授予");

    public static readonly Error LastProtectedHolder = Error.BusinessRule("Identity.LastProtectedHolder", "须保留至少一位可登录且持有已启用受保护角色的用户");

    public static readonly Error CannotActOnSelf = Error.Forbidden("Identity.CannotActOnSelf", "不能对自己执行此操作");

    /// <summary>命令复核操作者时发现授权已变更。</summary>
    public static readonly Error AuthorizationChanged = Error.Conflict("Identity.AuthorizationChanged", "当前授权已变化，请刷新后重试")
        .WithMetadata(ErrorMetadataKeys.Retryable, true);

    public static readonly Error PermissionUnknown = Error.Validation("Identity.PermissionUnknown", "权限点不存在或已废弃");

    public static readonly Error ScopeObjectUnknown = Error.Validation("Identity.ScopeObjectUnknown", "数据范围对象不存在");

    /// <summary>账号、密码无效或账号被禁用时统一返回，避免泄露账号是否存在。</summary>
    public static readonly Error InvalidCredentials = Error.Unauthenticated("Identity.InvalidCredentials", "账号或密码错误");

    /// <summary>验证码错误、已过期或尝试次数已耗尽。</summary>
    public static readonly Error InvalidSmsCode = Error.Unauthenticated("Identity.InvalidSmsCode", "验证码错误或已过期");

    public static readonly Error InvalidRefreshToken = Error.Unauthenticated("Identity.InvalidRefreshToken", "登录已失效，请重新登录");

    /// <summary>未过期的旧刷新令牌在宽限期外重用；仅内部流转，对外统一返回 InvalidRefreshToken。</summary>
    public static readonly Error RefreshTokenReused = Error.Unauthenticated("Identity.RefreshTokenReused", "登录已失效，请重新登录");

    public static readonly Error LoginThrottled = Error.TooManyRequests("Identity.LoginThrottled", "登录尝试过多，请稍后再试或联系管理员解锁", TimeSpan.FromMinutes(1));

    public static readonly Error SmsCodeThrottled = Error.TooManyRequests("Identity.SmsCodeThrottled", "验证码发送过于频繁，请稍后再试", TimeSpan.FromMinutes(1));

    public static readonly Error NoRoleAvailable = Error.Forbidden("Identity.NoRoleAvailable", "无可用角色，请联系管理员");

    /// <summary>目标角色未授予或已停用，或当前为模拟会话。</summary>
    public static readonly Error RoleNotSwitchable = Error.Forbidden("Identity.RoleNotSwitchable", "无法切换到该角色");

    public static readonly Error OldPasswordWrong = Error.Validation("Identity.OldPasswordWrong", "旧密码错误");

    public static readonly Error PasswordTooWeak = Error.Validation("Identity.PasswordTooWeak", "密码不符合安全要求：须满足长度要求，且不能为常见弱密码或与账号、手机号相同");

    public static readonly Error PasswordChangeRequired = Error.Forbidden("Identity.PasswordChangeRequired", "请先修改密码");

    public static readonly Error ImpersonationDenied = Error.Forbidden("Identity.ImpersonationDenied", "无法模拟该用户身份");

    public static readonly Error NotImpersonating = Error.BusinessRule("Identity.NotImpersonating", "当前会话不是模拟会话");

    public static readonly Error AlreadyBootstrapped = Error.Conflict("Identity.AlreadyBootstrapped", "系统中已有普通用户，不能再次初始化");

    /// <summary>刷新 Cookie 请求缺少可信 Origin 或必需的自定义请求头。</summary>
    public static readonly Error CsrfRejected = Error.Forbidden("Identity.CsrfRejected", "请求来源不可信");

    public static readonly Error NotProtectedHolder = Error.BusinessRule("Identity.NotProtectedHolder", "此账号未持有受保护角色，不能执行应急密码重置");
}
