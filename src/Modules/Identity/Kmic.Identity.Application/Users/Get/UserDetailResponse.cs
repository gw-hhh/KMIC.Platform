using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 用户详情
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version</param>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
/// <param name="Status">状态</param>
/// <param name="OrgUnitId">所属组织单元</param>
/// <param name="OrgUnitName">所属组织单元的名称</param>
/// <param name="HasPassword">是否设置过密码；迁移来的员工可能还没有，靠短信登录</param>
/// <param name="PasswordChangeRequired">下次登录是否必须先改密码</param>
/// <param name="PasswordChangedAt">密码最近一次变化的时间</param>
/// <param name="LoginLockedUntil">登录被限制到什么时候；没有被限制时为空。只影响新的登录尝试，不影响已有会话</param>
/// <param name="Grants">角色授予</param>
/// <param name="ActiveSessionCount">仍然有效的会话数，含他发起的模拟会话</param>
/// <param name="AllowedActions">当前用户现在可以对他执行的动作</param>
public sealed record UserDetailResponse(
    UserId Id,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    string Account,
    string DisplayName,
    string? EnglishName,
    string? EmployeeNo,
    string? Mobile,
    string? Email,
    UserStatus Status,
    OrgUnitId? OrgUnitId,
    string? OrgUnitName,
    bool HasPassword,
    bool PasswordChangeRequired,
    DateTimeOffset? PasswordChangedAt,
    DateTimeOffset? LoginLockedUntil,
    IReadOnlyList<UserGrantItem> Grants,
    int ActiveSessionCount,
    IReadOnlyList<string> AllowedActions);
