using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 查询用户详情：资料、授予、登录锁定、可以对他执行的动作
/// </summary>
/// <param name="Id">用户</param>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record GetUserQuery(UserId Id) : IQuery<UserDetailResponse>;
