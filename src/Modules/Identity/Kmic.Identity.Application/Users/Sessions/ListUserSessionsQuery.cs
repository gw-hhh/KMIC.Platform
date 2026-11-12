using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>
/// 查询用户当前有效的会话：设备、IP、时间；含他发起的模拟会话
/// </summary>
/// <param name="Id">用户</param>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record ListUserSessionsQuery(UserId Id) : IQuery<IReadOnlyList<UserSessionItem>>;
