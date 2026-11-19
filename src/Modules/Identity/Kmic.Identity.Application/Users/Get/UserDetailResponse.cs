using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 用户详情
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="ConcurrencyVersion">版本号</param>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
/// <param name="Status">状态</param>
/// <param name="OrgUnitId">所属组织单元 ID</param>
/// <param name="OrgUnitName">所属组织单元的名称</param>
/// <param name="HasPassword">是否已设置密码</param>
/// <param name="PasswordChangeRequired">下次登录是否必须先改密码</param>
/// <param name="PasswordChangedAt">最近密码修改时间</param>
/// <param name="LoginLockedUntil">登录等待或锁定的截止时间，无限制时为空</param>
/// <param name="Grants">角色授予</param>
/// <param name="ActiveSessionCount">未作废且未到绝对到期时间的会话数</param>
/// <param name="AllowedActions">当前用户可对该用户执行的管理动作</param>
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
