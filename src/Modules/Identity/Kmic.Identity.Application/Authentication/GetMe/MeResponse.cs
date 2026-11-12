namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>
/// 当前登录用户的信息
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="Account">账号</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="OrgUnitId">所属组织单元</param>
/// <param name="CurrentRole">当前角色</param>
/// <param name="Roles">可以切换的角色（启用的）</param>
/// <param name="Permissions">当前角色的权限点，按编码排序</param>
/// <param name="AuthorizationVersion">授权版本号：变化时前端重新拉取本接口</param>
/// <param name="PasswordChangeRequired">是否必须先修改密码</param>
/// <param name="HasPassword">是否已经设置过密码：没有时修改密码不要求旧密码</param>
/// <param name="Impersonating">是否正在模拟身份</param>
/// <param name="ImpersonatorUserId">模拟身份时的实际操作人</param>
/// <param name="ImpersonationAllowsWrite">模拟会话是否允许修改数据</param>
/// <param name="BusinessTimeZone">业务时区（IANA ID），前端统一按它显示时刻</param>
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
