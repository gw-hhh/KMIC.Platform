using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Enable;

/// <summary>
/// 启用用户
/// </summary>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record EnableUserCommand(UserId Id, long ExpectedVersion) : ICommand<VersionedResponse<UserId>>;
