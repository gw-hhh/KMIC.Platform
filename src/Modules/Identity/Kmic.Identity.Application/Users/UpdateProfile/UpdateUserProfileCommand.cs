using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.UpdateProfile;

/// <summary>
/// 修改用户资料
/// </summary>
/// <remarks>
/// 改受保护角色持有人的账号或手机号时，操作者当前角色也须受保护，其他资料不受限
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回版本冲突</param>
/// <param name="Account">登录名</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号</param>
/// <param name="Mobile">手机号</param>
/// <param name="Email">邮箱</param>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record UpdateUserProfileCommand(UserId Id, long ExpectedVersion, string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email)
    : ICommand<VersionedResponse<UserId>>;
