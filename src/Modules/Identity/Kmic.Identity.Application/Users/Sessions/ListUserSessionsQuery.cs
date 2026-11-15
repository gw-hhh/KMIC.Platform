using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>
/// 查询用户未作废且未到绝对到期时间的会话，含相关模拟会话
/// </summary>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record ListUserSessionsQuery(UserId Id) : IQuery<IReadOnlyList<UserSessionItem>>;
