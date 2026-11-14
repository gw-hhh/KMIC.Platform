using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>
/// 查询用户当前有效的会话，包含以其身份创建及由其发起的模拟会话
/// </summary>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record ListUserSessionsQuery(UserId Id) : IQuery<IReadOnlyList<UserSessionItem>>;
