using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.UnlockLogin;

/// <summary>
/// 解锁登录：清零这个账号的登录失败计数
/// </summary>
/// <remarks>
/// 登录限制只影响新的登录尝试，不影响用户已有的会话，所以解锁不会改动用户本身
/// </remarks>
/// <param name="Id">用户</param>
[RequirePermission(IdentityPermissions.UserUnlock)]
[Audited]
[Idempotent]
public sealed record UnlockLoginCommand(UserId Id) : ICommand<VersionedResponse<UserId>>;
