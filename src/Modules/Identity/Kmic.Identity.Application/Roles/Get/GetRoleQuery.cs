using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>查询角色详情及可用操作。</summary>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record GetRoleQuery(RoleId Id) : IQuery<RoleDetailResponse>;
