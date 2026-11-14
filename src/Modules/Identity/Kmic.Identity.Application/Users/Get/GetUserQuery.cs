using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 查询用户详情及可用操作
/// </summary>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record GetUserQuery(UserId Id) : IQuery<UserDetailResponse>;
