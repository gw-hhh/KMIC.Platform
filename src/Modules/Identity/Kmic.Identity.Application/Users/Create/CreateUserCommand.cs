using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Create;

/// <summary>
/// 新建用户：不设置密码、所属组织和角色，这些由各自的命令设置
/// </summary>
/// <param name="Account">登录名，不区分大小写唯一</param>
/// <param name="DisplayName">姓名</param>
/// <param name="EnglishName">英文名</param>
/// <param name="EmployeeNo">工号，填写时唯一</param>
/// <param name="Mobile">手机号，填写时唯一</param>
/// <param name="Email">邮箱</param>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record CreateUserCommand(string Account, string DisplayName, string? EnglishName, string? EmployeeNo, string? Mobile, string? Email) : ICommand<CreatedResponse<UserId>>;
