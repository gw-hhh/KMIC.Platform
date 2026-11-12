using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.UpdateProfile;

/// <summary>
/// 修改用户资料
/// </summary>
/// <remarks>
/// 账号和手机号是登录凭据：对持有受保护角色的用户，只有同样持有受保护角色的人才能改这两项，其余资料不受这条限制
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
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
