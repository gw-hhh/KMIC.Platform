using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 授权诊断：用户在某个角色下的权限和数据范围
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="ScopeObject">范围对象编码，为空时不返回数据范围</param>
[RequirePermission(IdentityPermissions.AuthorizationExplain)]
[AuditedRead]
public sealed record ExplainAuthorizationQuery(UserId UserId, RoleId RoleId, string? ScopeObject) : IQuery<AuthorizationExplanation>;
