using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.List;

/// <summary>
/// 用户列表的一行
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Status">状态</param>
/// <param name="OrgUnitId">所属组织单元</param>
/// <param name="OrgUnitName">所属组织单元的名称</param>
/// <param name="Roles">拥有的角色</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version</param>
public sealed record UserListItem(
    UserId Id,
    string Account,
    string DisplayName,
    string? EmployeeNo,
    string? Mobile,
    UserStatus Status,
    OrgUnitId? OrgUnitId,
    string? OrgUnitName,
    IReadOnlyList<UserRoleBrief> Roles,
    [property: JsonPropertyName("version")] long ConcurrencyVersion);
