using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 查询角色详情：权限点、数据范围配置、持有人数和当前用户可用的动作
/// </summary>
/// <param name="Id">角色</param>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record GetRoleQuery(RoleId Id) : IQuery<RoleDetailResponse>;
