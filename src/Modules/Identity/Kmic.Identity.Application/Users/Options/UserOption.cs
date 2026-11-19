using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Options;

/// <summary>
/// 用户选择项
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="OrgUnitName">所属组织单元的名称</param>
/// <param name="Status">用户状态</param>
public sealed record UserOption(UserId Id, string DisplayName, string? EmployeeNo, string? OrgUnitName, UserStatus Status);
