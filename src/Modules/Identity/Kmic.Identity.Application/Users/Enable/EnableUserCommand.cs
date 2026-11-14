using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Enable;

/// <summary>
/// 启用用户
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回版本冲突</param>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record EnableUserCommand(UserId Id, long ExpectedVersion) : ICommand<VersionedResponse<UserId>>;
