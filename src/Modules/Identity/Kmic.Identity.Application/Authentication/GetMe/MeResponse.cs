namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>当前登录用户信息</summary>
/// <param name="UserId">用户 ID</param>
/// <param name="Account">账号</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="OrgUnitId">所属组织单元 ID</param>
/// <param name="CurrentRole">当前角色</param>
/// <param name="Roles">已获授且启用的角色</param>
/// <param name="Permissions">当前角色的权限点</param>
/// <param name="AuthorizationVersion">授权版本号</param>
/// <param name="PasswordChangeRequired">是否必须先修改密码</param>
/// <param name="HasPassword">是否已设置密码</param>
/// <param name="Impersonating">是否正在模拟身份</param>
/// <param name="ImpersonatorUserId">实际操作人 ID</param>
/// <param name="ImpersonationAllowsWrite">模拟会话是否允许写入</param>
/// <param name="BusinessTimeZone">业务时区（IANA ID）</param>
public sealed record MeResponse(
    Guid UserId,
    string Account,
    string DisplayName,
    string? EnglishName,
    Guid? OrgUnitId,
    MeRole CurrentRole,
    IReadOnlyList<MeRole> Roles,
    IReadOnlyList<string> Permissions,
    long AuthorizationVersion,
    bool PasswordChangeRequired,
    bool HasPassword,
    bool Impersonating,
    Guid? ImpersonatorUserId,
    bool ImpersonationAllowsWrite,
    string BusinessTimeZone);
