using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.UnlockLogin;

/// <summary>清除账号的登录失败计数。</summary>
[RequirePermission(IdentityPermissions.UserUnlock)]
[Audited]
[Idempotent]
public sealed record UnlockLoginCommand(UserId Id) : ICommand<VersionedResponse<UserId>>;
